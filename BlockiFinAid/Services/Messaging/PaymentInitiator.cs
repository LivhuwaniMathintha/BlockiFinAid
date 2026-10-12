using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services.Observability;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.Payment;
using BlockiFinAid.Services.SmartContracts.User;

namespace BlockiFinAid.Services.Messaging;

public class PaymentEvent 
{
    public string FromAccountNumber { get; set; } = string.Empty;
    public string FromBankName { get; set; } = string.Empty;
    public string FromBankBranchCode { get; set; } = string.Empty;
    public string ToAccountNumber { get; set; } = string.Empty;
    public string ToBankName { get; set; }  = string.Empty;
    public string ToBankBranchCode { get; set; } = string.Empty;
    public bool IsPaid { get; set; } = false;
    public DateTime InitiatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime FulfillmentDate { get; set; }
    public string Status { get; set; } = "Initiated";
    public bool IsFraud { get; set; } = false;
    public string PaymentType { get; set; } = "Food";
    public decimal Amount { get; set; } = 0;

}

public class PaymentInitiator : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MongoDbBackgroundPublisherService> _logger;
    private readonly OperationTimingTracker _timingTracker;
    private readonly IConfiguration _configuration;
    private readonly SeedingLifecycle _seedingLifecycle;


    public PaymentInitiator(IServiceScopeFactory scopeFactory, ILogger<MongoDbBackgroundPublisherService> logger, OperationTimingTracker timingTracker, IConfiguration configuration, SeedingLifecycle seedingLifecycle)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timingTracker = timingTracker;
        _configuration = configuration;
        _seedingLifecycle = seedingLifecycle;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_configuration.GetValue<bool>("DemoSimulation:Enabled"))
        {
            _logger.LogInformation(
                "Payment initiator is disabled while DemoSimulation is enabled; demo payments are handled without real contract transactions.");
            return;
        }

        var forceTestMode = _configuration.GetValue<bool>("PaymentInitiator:ForceTestMode");
        var startupDelay = forceTestMode ? TimeSpan.FromSeconds(5) : TimeSpan.FromMinutes(1);

        if (startupDelay > TimeSpan.Zero)
        {
            await Task.Delay(startupDelay, stoppingToken);
        }

        _logger.LogInformation("Waiting for initial seed process to complete before checking for due payments.");
        await _seedingLifecycle.WaitUntilCompleteAsync(stoppingToken);
        _logger.LogInformation("Initial seeding completed; payment initiator is now active.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting to watch for payments that needs to be processed within the next 48 hours");
                await ProcessDuePaymentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment initiator loop failed.");
            }

            var nextDelay = forceTestMode ? TimeSpan.FromSeconds(30) : TimeSpan.FromMinutes(2);
            await Task.Delay(nextDelay, stoppingToken);
        }
    }

    private async Task ProcessDuePaymentsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var funderContract = scope.ServiceProvider.GetRequiredService<FunderContract>();
        var bankingAccountContract = scope.ServiceProvider.GetRequiredService<BankAccountContract>();
        var usersContract = scope.ServiceProvider.GetRequiredService<UserContract>();
        var contractConditions = scope.ServiceProvider.GetRequiredService<FundingConditionsContract>();
        var funding = scope.ServiceProvider.GetRequiredService<FundingContract>();
        var paymentModel = scope.ServiceProvider.GetRequiredService<IBaseRepository<PaymentModel>>();
        var paymentCheckerRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<PaymentCheckerModel>>();
        var forceTestMode = _configuration.GetValue<bool>("PaymentInitiator:ForceTestMode");
        var forceTestDateText = _configuration["PaymentInitiator:ForceTestDateUtc"];
        var twoDaysFromNow = DateTime.UtcNow.AddDays(2);
        var currentMonth = DateTime.UtcNow.Month;
        var allFunders = await funderContract.GetAllFundersAsync();
        _logger.LogInformation("[Payment Initiator] - Found {count} Funders in the smart contract", allFunders.Count());
        foreach (var funder in allFunders)
        {
            var funderStartedAt = DateTime.UtcNow;
            var now = DateTime.UtcNow;
            var paymentChecker = await paymentCheckerRepo.GetAllAsync();
            DateTime.TryParse(funder.PaymentDate, out DateTime paymentDate);
            if (forceTestMode && DateTime.TryParse(forceTestDateText, out var parsedForceDate))
            {
                paymentDate = parsedForceDate;
            }
            paymentDate = new DateTime(paymentDate.Year, currentMonth, paymentDate.Day);
            var paymentDateKey = new DateTime(now.Year, now.Month, paymentDate.Day);
            var lastPaymentDate = paymentChecker.FirstOrDefault(x => x.LastPaidDate.Date == paymentDateKey.Date && x.FunderId == funder.Id);
            var shouldProcessPayment = forceTestMode || paymentDate.Date == twoDaysFromNow.Date;
            if (forceTestMode)
            {
                _logger.LogWarning("[Payment Initiator] Force mode is active for {FunderName}; bypassing the normal schedule but retaining duplicate-payment checks.", funder.Name);
            }

            if (lastPaymentDate != null)
            {
                _logger.LogInformation("[Payment Initiator] - Processing marker for {FunderName} on {PaymentDate:yyyy-MM-dd} already exists. Skipping duplicate creation.", funder.Name, paymentDateKey);
                var skippedElapsedMs = (long)(DateTime.UtcNow - funderStartedAt).TotalMilliseconds;
                _timingTracker.RecordStep("payment_creation", "FunderModel", "total_processing", skippedElapsedMs, funder.Id.ToString());
                continue;
            }

            if (lastPaymentDate == null)
            {
                if (shouldProcessPayment)
                {
                    _logger.LogInformation(
                        $"[Payment Initiator] - {funder.Name} has a payment date in the next 48 hours: {twoDaysFromNow}. Initiating processing");
                    var getFunderUser = await usersContract.GetUserByNameAsync(funder.Name);

                    if (getFunderUser != null)
                    {
                        var bankAccounts = await bankingAccountContract.GetByStudentNumberAsync(getFunderUser.StudentNumber);
                        _logger.LogInformation(
                            $"[Payment Initiator] - Found user for {funder.Name} with Student Number of {getFunderUser.StudentNumber}");
                        if (bankAccounts is not null)
                        {
                            _logger.LogInformation(
                                $"[Payment Initiator] - Found bank account for {funder.Name} with Student Number of {getFunderUser.StudentNumber}");
                            var allStudents = await usersContract.GetAllUsersAsync();
                            var funderStudents = allStudents.FindAll(x => x.FunderContractId == funder.Id);
                            _logger.LogInformation(
                                $"Found {funderStudents.Count} students for {funder.Name} from {allStudents.Count} overall students");
                            var paymentsCreated = false;
                            foreach (var student in funderStudents)
                            {
                                _logger.LogInformation("[Payment Initiator] - Processing payment for student {StudentName} with Student Number {StudentNumber} has started", student.Name, student.StudentNumber);
                                _logger.LogInformation(
                                    $"[Payment Initiator] - Found student {student.Name}. Retrieving Student Bank Account Details");
                                var studentBankDetails =
                                    await bankingAccountContract.GetByStudentNumberAsync(student.StudentNumber);

                                if (studentBankDetails is not null)
                                {
                                    _logger.LogInformation(
                                        $"[Payment Initiator] - Found Banking Details for {student.Name} with Student Number of {student.StudentNumber}. Retriving Funding Contract");

                                    var studentFunding = await funding.GetFundingByStudentIdAsync(student.Id);

                                    if (studentFunding is not null)
                                    {
                                        _logger.LogInformation(
                                            $"[Payment Initiator] - Found Funding Contract for {student.Name} with Student Number of {student.StudentNumber}");

                                        var fundingConditions =
                                            await contractConditions.GetConditionByIdAsync(
                                                studentFunding.FunderContractConditionId.ToString());

                                        if (fundingConditions is not null)
                                        {
                                            _logger.LogInformation(
                                                $"[Payment Initiator] - Found Funding Conditions Contract for {student.Name} with Student Number of {student.StudentNumber}");

                                            var fundingContract = await funding.GetFundingByStudentIdAsync(student.Id);

                                            if (fundingContract is not null)
                                            {
                                                var newPaymentTrx = "";
                                                var trxId = "";
                                                var trxExists = false;
                                                var allPayments = await paymentModel.GetAllAsync();
                                                do
                                                {
                                                    newPaymentTrx = UAMHelper.GeneratePaymentGroupNumber();
                                                    var check = allPayments.Any(x => x.TransactionGroupId == newPaymentTrx);
                                                    if (!check)
                                                    {
                                                        trxExists = true;
                                                    }
                                                } while (trxExists == false);

                                                trxExists = false;
                                                do
                                                {
                                                    trxId = UAMHelper.GeneratePaymentGroupNumber();
                                                    var check = allPayments.Any(x => x.TransactionId == trxId);
                                                    if (!check)
                                                    {
                                                        trxExists = true;
                                                    }
                                                } while (trxExists == false);

                                                var paymentEvent = new PaymentModel
                                                {
                                                    Id = Guid.NewGuid(),
                                                    AccountNumber = studentBankDetails.BankAccountNumber,
                                                    BankName = studentBankDetails.BankName,
                                                    BranchCode = studentBankDetails.BankBranchCode,
                                                    FulfilmentDate = DateTime.UtcNow.AddDays(2),
                                                    InitiationDate = DateTime.UtcNow,
                                                    InstitutionServiceId = getFunderUser.Id,
                                                    IsFraud = false,
                                                    ModifiedBy = "api-admin",
                                                    UpdatedAt = DateTime.UtcNow,
                                                    Status = "Initiated",
                                                    Institution = "University of Johannesburg",
                                                    StudentId = student.Id,
                                                    StudentNumber = student.StudentNumber,
                                                    Funder = funder.Name,
                                                    TransactionGroupId = newPaymentTrx,
                                                    TransactionId = trxId,
                                                };

                                                if (fundingContract.FoodBalance > 0)
                                                {
                                                    var monthlyAmount = fundingConditions.FoodAmount / 10;
                                                    var fundingUpdateDto = new FundingUpdateDto();
                                                    if (fundingContract.FoodBalance > monthlyAmount)
                                                    {
                                                        paymentEvent.Amount = monthlyAmount;
                                                        paymentEvent.PaymentType = "Food";
                                                        var beforeMongoInsert = DateTime.UtcNow;
                                                        var createdPayment = await paymentModel.CreateAsync(paymentEvent, "sys-api");
                                                        paymentsCreated |= createdPayment is PaymentModel;
                                                        var mongoInsertMs = (long)(DateTime.UtcNow - beforeMongoInsert).TotalMilliseconds;
                                                        _timingTracker.RecordStep("payment_creation", "PaymentModel", "mongo_insert", mongoInsertMs, paymentEvent.Id.ToString());
                                                        fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                        fundingUpdateDto.IsActive = true;
                                                        fundingUpdateDto.FoodBalance = fundingContract.FoodBalance - monthlyAmount;
                                                        fundingUpdateDto.ModifiedBy = "api-admin";
                                                        await funding.UpdateFundingAsync(fundingUpdateDto);
                                                    }
                                                }

                                                if (fundingContract.AccommodationBalance > 0)
                                                {
                                                    var monthlyAmount = fundingConditions.AccommodationAmount / 10;
                                                    var fundingUpdateDto = new FundingUpdateDto();
                                                    if (fundingContract.AccommodationBalance > monthlyAmount)
                                                    {
                                                        paymentEvent.Amount = monthlyAmount;
                                                        paymentEvent.Id = Guid.NewGuid();
                                                        paymentEvent.PaymentType = "Accommodation";
                                                        var beforeMongoInsert = DateTime.UtcNow;
                                                        var createdPayment = await paymentModel.CreateAsync(paymentEvent, "sys-api");
                                                        paymentsCreated |= createdPayment is PaymentModel;
                                                        var mongoInsertMs = (long)(DateTime.UtcNow - beforeMongoInsert).TotalMilliseconds;
                                                        _timingTracker.RecordStep("payment_creation", "PaymentModel", "mongo_insert", mongoInsertMs, paymentEvent.Id.ToString());
                                                        fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                        fundingUpdateDto.IsActive = true;
                                                        fundingUpdateDto.AccommodationBalance = fundingContract.FoodBalance - monthlyAmount;
                                                        fundingUpdateDto.ModifiedBy = "api-admin";
                                                        await funding.UpdateFundingAsync(fundingUpdateDto);
                                                    }
                                                }

                                                if (fundingContract.LaptopBalance > 0)
                                                {
                                                    var monthlyAmount = fundingConditions.LaptopAmount / 10;
                                                    var fundingUpdateDto = new FundingUpdateDto();
                                                    if (fundingContract.LaptopBalance > monthlyAmount)
                                                    {
                                                        paymentEvent.Amount = monthlyAmount;
                                                        paymentEvent.Id = Guid.NewGuid();
                                                        paymentEvent.PaymentType = "Laptop/Book";
                                                        var beforeMongoInsert = DateTime.UtcNow;
                                                        var createdPayment = await paymentModel.CreateAsync(paymentEvent, "sys-api");
                                                        paymentsCreated |= createdPayment is PaymentModel;
                                                        var mongoInsertMs = (long)(DateTime.UtcNow - beforeMongoInsert).TotalMilliseconds;
                                                        _timingTracker.RecordStep("payment_creation", "PaymentModel", "mongo_insert", mongoInsertMs, paymentEvent.Id.ToString());
                                                        fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                        fundingUpdateDto.IsActive = true;
                                                        fundingUpdateDto.LaptopBalance = fundingContract.LaptopBalance - monthlyAmount;
                                                        fundingUpdateDto.ModifiedBy = "api-admin";
                                                        await funding.UpdateFundingAsync(fundingUpdateDto);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (paymentsCreated)
                            {
                                var checkerStartedAt = DateTime.UtcNow;
                                await paymentCheckerRepo.CreateAsync(new PaymentCheckerModel
                                {
                                    Id = Guid.NewGuid(),
                                    LastPaidDate = paymentDateKey,
                                    FunderId = funder.Id
                                }, "sys-admin");
                                var checkerElapsedMs = (long)(DateTime.UtcNow - checkerStartedAt).TotalMilliseconds;
                                _timingTracker.RecordStep("payment_creation", "PaymentCheckerModel", "mongo_insert", checkerElapsedMs, funder.Id.ToString());
                            }
                            if (!allStudents.Any())
                            {
                                _logger.LogError(
                                    $"[Payment Initiator] - Could not find students belonging to {funder.Name} therefore, cannot proceed with payment generation");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError(
                            $"[Payment Initiator] - Could not find any user for {funder.Name} and as such cannot retrieve any banking details. make sure user and bank details for the funder are added to the system.");
                    }
                }
                else
                {
                    _logger.LogInformation(
                        $"[Payment Initiator] - Payment Date: {funder.PaymentDate} for {funder.Name} is not in the next 48 hours. No further processing required.");
                }
            }
            else
            {
                _logger.LogInformation($"[Payment Initiator] - Payment for this month for this {funder.Name} has already been processed");
            }
            var funderElapsedMs = (long)(DateTime.UtcNow - funderStartedAt).TotalMilliseconds;
            _timingTracker.RecordStep("payment_creation", "FunderModel", "total_processing", funderElapsedMs, funder.Id.ToString());
        }
    }
}