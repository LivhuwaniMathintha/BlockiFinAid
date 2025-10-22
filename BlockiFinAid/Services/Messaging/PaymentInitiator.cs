using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
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


    public PaymentInitiator(IServiceScopeFactory scopeFactory, ILogger<MongoDbBackgroundPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        _logger.LogInformation("Starting to watch for payments that needs to be processed within the next 48 hours");
        using var scope = _scopeFactory.CreateScope();
        var funderContract = scope.ServiceProvider.GetRequiredService<FunderContract>();
        var bankingAccountContract = scope.ServiceProvider.GetRequiredService<BankAccountContract>();
        var usersContract = scope.ServiceProvider.GetRequiredService<UserContract>();
        var contractConditions = scope.ServiceProvider.GetRequiredService<FundingConditionsContract>();
        var funding = scope.ServiceProvider.GetRequiredService<FundingContract>();
        var paymentPublisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var paymentContract = scope.ServiceProvider.GetRequiredService<PaymentContract>();
        var paymentModel = scope.ServiceProvider.GetRequiredService<IBaseRepository<PaymentModel>>();
        var paymentCheckerRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<PaymentCheckerModel>>();
        var twoDaysFromNow = DateTime.UtcNow.AddDays(2);
        var currentMonth = DateTime.UtcNow.Month;
        var allFunders = await funderContract.GetAllFundersAsync();
        _logger.LogInformation("[Payment Initiator] - Found {count} Funders in the smart contract", allFunders.Count());
        foreach (var funder in allFunders)
        {
            var now = DateTime.UtcNow;
            var paymentChecker = await paymentCheckerRepo.GetAllAsync();
            DateTime.TryParse(funder.PaymentDate, out DateTime paymentDate);
            paymentDate = new DateTime(paymentDate.Year, currentMonth, paymentDate.Day);
            var lastPaymentDate = paymentChecker.FirstOrDefault(x => x.LastPaidDate == new DateTime(now.Year, now.Month, paymentDate.Day) && x.FunderId == funder.Id);
            // var checker = await paymentCheckerRepo.CreateAsync(new PaymentCheckerModel
            // {
            //     Id = Guid.NewGuid(),
            //     LastPaidDate = new DateTime(now.Year, now.Month, paymentDate.Day),
            //     FunderId = Guid.NewGuid()
            // }, "sys-admin");
            if (lastPaymentDate == null)
            {
                if (paymentDate.Date == twoDaysFromNow.Date)
                {
                    // Process this payment. Publish to rabbit MQ for processing.
                    _logger.LogInformation(
                        $"[Paymwent Initiator] - {funder.Name} has a payment date in the next 48 hours: {twoDaysFromNow}. Initiating processing");
                    // We need to get this funder's bank Details
                    var getFunderUser = await usersContract.GetUserByNameAsync(funder.Name);

                    if (getFunderUser != null)
                    {
                        var bankAccounts = await bankingAccountContract
                            .GetByStudentNumberAsync(getFunderUser.StudentNumber);
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
                            foreach (var student in funderStudents)
                            {
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
                                                // var paymentEvent = new PaymentEvent
                                                // {
                                                //     FromAccountNumber = bankAccounts.BankAccountNumber,
                                                //     FromBankName = bankAccounts.BankName,
                                                //     FromBankBranchCode = bankAccounts.BankBranchCode,
                                                //     ToAccountNumber = studentBankDetails.BankAccountNumber,
                                                //     ToBankName = studentBankDetails.BankName,
                                                //     ToBankBranchCode = studentBankDetails.BankBranchCode
                                                // };
                                                var newPaymentTrx = "";
                                                var trxId = "";
                                                var trxExists = false;
                                                var allPayments = await paymentModel.GetAllAsync();
                                                do
                                                {
                                                    newPaymentTrx = UAMHelper.GeneratePaymentGroupNumber();
                                                    _logger.LogInformation(
                                                        $"Generated TransactionGroupId:  {newPaymentTrx}");
                                                    var check = allPayments.Any(x =>
                                                        x.TransactionGroupId == newPaymentTrx);
                                                    if (!check)
                                                    {
                                                        _logger.LogInformation(
                                                            "[Payment Initiator] - TransactionGroupId not in Database. Proceeding with Generating Payment");
                                                        trxExists = true;
                                                    }
                                                } while (trxExists == false);

                                                trxExists = false;
                                                do
                                                {
                                                    trxId = UAMHelper.GeneratePaymentGroupNumber();
                                                    _logger.LogInformation($"Generated TransactionId: {trxId}");
                                                    var check = allPayments.Any(x => x.TransactionId == trxId);

                                                    if (!check)
                                                    {
                                                        _logger.LogInformation(
                                                            "[Payment Initiator] - TransactionId not in Database. Proceeding with Generating Payment");
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

                                                // for food

                                                if (fundingContract.FoodBalance > 0)
                                                {
                                                    var monthlyAmount = fundingConditions.FoodAmount / 10;
                                                    var fundingUpdateDto = new FundingUpdateDto();
                                                    if (fundingContract.FoodBalance > monthlyAmount)
                                                    {
                                                        paymentEvent.Amount = monthlyAmount;

                                                        paymentEvent.PaymentType = "Food";
                                                        await paymentModel.CreateAsync(paymentEvent, "sys-api");

                                                        // subtract from the payment balance
                                                        _logger.LogInformation(
                                                            $"[Paymwent Initiator] - Payment for {paymentEvent.PaymentType} has been added to the mongo db and will be pulled to smart contract next.");

                                                        fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                        fundingUpdateDto.IsActive = true;
                                                        fundingUpdateDto.FoodBalance =
                                                            fundingContract.FoodBalance - monthlyAmount;
                                                        fundingUpdateDto.ModifiedBy = "api-admin";

                                                        // update the funding
                                                        await funding.UpdateFundingAsync(fundingUpdateDto);

                                                    }
                                                    else
                                                    {
                                                        _logger.LogError(
                                                            $"[Paymwent Initiator] - Payment event for {student.Name} for food could not be processed because of insufficient monthly balance of ZAR {fundingContract.FoodBalance}");
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
                                                        //publish this event
                                                        paymentEvent.PaymentType = "Accommodation";

                                                        await paymentModel.CreateAsync(paymentEvent, "sys-api");
                                                        // subtract from the payment balance
                                                        _logger.LogInformation(
                                                            $"[Paymwent Initiator] - Payment for {paymentEvent.PaymentType} has been added to the mongo db and will be pulled to smart contract next.");

                                                        fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                        fundingUpdateDto.IsActive = true;
                                                        fundingUpdateDto.AccommodationBalance =
                                                            fundingContract.FoodBalance - monthlyAmount;
                                                        fundingUpdateDto.ModifiedBy = "api-admin";
                                                        // update the funding
                                                        await funding.UpdateFundingAsync(fundingUpdateDto);
                                                    }
                                                    else
                                                    {
                                                        _logger.LogError(
                                                            $"[Paymwent Initiator] - Payment event for {student.Name}  for accommodation could not be processed because of insufficient monthly balance of ZAR {fundingContract.FoodBalance}");
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
                                                        //publish this event
                                                        await paymentModel.CreateAsync(paymentEvent, "sys-api");
                                                        // subtract from the payment balance
                                                        _logger.LogInformation(
                                                            $"[Paymwent Initiator] - Payment for {paymentEvent.PaymentType} has been added to the mongo db and will be pulled to smart contract next.");
                                                        fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                        fundingUpdateDto.IsActive = true;
                                                        fundingUpdateDto.LaptopBalance =
                                                            fundingContract.LaptopBalance - monthlyAmount;
                                                        fundingUpdateDto.ModifiedBy = "api-admin";
                                                        // update the funding
                                                        await funding.UpdateFundingAsync(fundingUpdateDto);
                                                    }
                                                    else
                                                    {
                                                        _logger.LogError(
                                                            $"[Paymwent Initiator] - Payment event for {student.Name}  for accommodation could not be processed because of insufficient monthly balance of ZAR {fundingContract.FoodBalance}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogError(
                                                $"[Payment Initiator] - Funding Conditions Contract for student {student.StudentNumber} not found");
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogError(
                                            $"[Payment Initiator] - Funding Contract for student {student.StudentNumber} not found");
                                    }
                                }
                                else
                                {
                                    _logger.LogError(
                                        $"[Payment Initiator] - Bank account for {student.Name} not Found");
                                }
                            }
                            var checker = await paymentCheckerRepo.CreateAsync(new PaymentCheckerModel
                            {
                                Id = Guid.NewGuid(),
                                LastPaidDate = new DateTime(now.Year, now.Month, paymentDate.Day),
                                FunderId = funder.Id
                            }, "sys-admin");
                            if (!allStudents.Any())
                            {
                                _logger.LogError(
                                    $"[Paymwent Initiator] - Could not find students belonging to {funder.Name} therefore, cannot proceed with payment generation");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError(
                            $"[Paymwent Initiator] - Could not find any user for {funder.Name} and as such cannot retrieve any banking details. make sure user and bank details for the funder are added to the system.");
                    }
                }

                _logger.LogInformation(
                    $"[Paymwent Initiator] - Payment Date: {funder.PaymentDate} for {funder.Name} is not in the next 48 hours. No further processing required.");
            }

            else
            {
                _logger.LogInformation($"[Payment Initiator] - Payment for this month for this {funder.Name} has already been processed");
            }
        }
    }
}