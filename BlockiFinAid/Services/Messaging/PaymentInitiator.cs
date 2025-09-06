using BlockiFinAid.Data.Models;
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
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
        _logger.LogInformation("Starting to watch for payments that needs to be processed within the next 48 hours");
        using var scope = _scopeFactory.CreateScope();
        var funderContract = scope.ServiceProvider.GetRequiredService<FunderContract>();
        var bankingAccountContract = scope.ServiceProvider.GetRequiredService<BankAccountContract>();
        var usersContract = scope.ServiceProvider.GetRequiredService<UserContract>();
        var contractConditions = scope.ServiceProvider.GetRequiredService<FundingConditionsContract>();
        var funding = scope.ServiceProvider.GetRequiredService<FundingContract>();
        var paymentPublisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var paymentContract = scope.ServiceProvider.GetRequiredService<PaymentContract>();
        var twoDaysFromNow = DateTime.UtcNow.AddDays(2);
        var currentMonth = DateTime.UtcNow.Month;
        var allFunders = await funderContract.GetAllFundersAsync();
        _logger.LogInformation("Found {count} Funders in the smart contract", allFunders.Count());
        foreach (var funder in allFunders)
        {
            DateTime.TryParse(funder.PaymentDate, out DateTime paymentDate);
            paymentDate = new DateTime(paymentDate.Year, currentMonth, paymentDate.Day);
            if (paymentDate.Date == twoDaysFromNow.Date)
            {
                // Process this payment. Publish to rabbit MQ for processing.
                _logger.LogInformation($"{funder.Name} has a payment date in the next 48 hours: {twoDaysFromNow}. Initiating processing");
                // We need to get this funder's bank Details
                var getFunderUser = await usersContract.GetUserByNameAsync(funder.Name);

                if (getFunderUser != null)
                {
                    var bankAccounts = await bankingAccountContract
                        .GetByStudentNumberAsync(getFunderUser.StudentNumber);
                    _logger.LogInformation($"Found user for {funder.Name} with Student Number of {getFunderUser.StudentNumber}");
                    if(bankAccounts is not null)
                    {
                        _logger.LogInformation($"Found bank account for {funder.Name} with Student Number of {getFunderUser.StudentNumber}");
                        var allStudents = await usersContract.GetAllUsersAsync();

                        foreach (var student in allStudents.Where(x => x.FunderContractId == funder.Id))
                        {
                            var studentBankDetails =
                                await bankingAccountContract.GetByStudentNumberAsync(student.StudentNumber);

                            if (studentBankDetails is not null)
                            {
                                var fundingConditions = await contractConditions.GetConditionByIdAsync(student.Id.ToString());

                                if (fundingConditions is not null)
                                {
                                    var fundingContract = await funding.GetFundingByStudentIdAsync(student.Id);

                                    if (fundingContract is not null)
                                    {
                                        var paymentEvent = new PaymentEvent
                                        {
                                            FromAccountNumber = bankAccounts.BankAccountNumber,
                                            FromBankName = bankAccounts.BankName,
                                            FromBankBranchCode = bankAccounts.BankBranchCode,
                                            ToAccountNumber = studentBankDetails.BankAccountNumber,
                                            ToBankName = studentBankDetails.BankName,
                                            ToBankBranchCode = studentBankDetails.BankBranchCode
                                        };

                                        // for food

                                        if (fundingContract.FoodBalance > 0)
                                        {
                                            var monthlyAmount = fundingConditions.FoodAmount / 10;
                                            var fundingUpdateDto = new FundingUpdateDto();
                                            if (fundingContract.FoodBalance > monthlyAmount)
                                            {
                                                paymentEvent.Amount = monthlyAmount;
                                                
                                                //publish this event
                                                await paymentPublisher.Publish(paymentEvent);
                                                var results = await paymentContract.AddPaymentAsync(new PaymentInputDto()
                                                {
                                                    AccountNumber = paymentEvent.ToAccountNumber,
                                                    Amount = monthlyAmount,
                                                    BankName = paymentEvent.ToBankName,
                                                    BranchCode = paymentEvent.ToBankBranchCode,
                                                    FulfilmentDate = "",
                                                    InitiationDate = paymentEvent.InitiatedDateTime.ToString("G"),
                                                    IsFraud = false,
                                                    ModifiedBy = "api-admin",
                                                    PaymentType = paymentEvent.PaymentType,
                                                });
                                                // subtract from the payment balance
                                                _logger.LogInformation($"Payment has been added to the smart contract in block: {results.BlockNumber}, function sighashes: {results.FunctionSigHashes}, timestamp: {results.BlockHash} on address: {results.ContractAddress}");
                                                
                                                fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                fundingUpdateDto.IsActive = true;
                                                fundingUpdateDto.FoodBalance = fundingContract.FoodBalance - monthlyAmount;
                                                fundingUpdateDto.ModifiedBy = "api-admin";
                                                
                                                // update the funding
                                                await funding.UpdateFundingAsync(fundingUpdateDto);
                                                
                                                
                                            }
                                            else
                                            {
                                                _logger.LogError($"Payment event for {student.Name} for food could not be processed because of insufficient monthly balance of ZAR {fundingContract.FoodBalance}");
                                            }
                                        }

                                        if (fundingContract.AccommodationBalance > 0)
                                        {
                                            var monthlyAmount = fundingConditions.AccommodationAmount / 10;
                                            var fundingUpdateDto = new FundingUpdateDto();
                                            if (fundingContract.AccommodationBalance > monthlyAmount)
                                            {
                                                paymentEvent.Amount = monthlyAmount;
                                                
                                                //publish this event
                                                paymentEvent.PaymentType = "Accommodation";
                                                await paymentPublisher.Publish(paymentEvent);
                                                var results = await paymentContract.AddPaymentAsync(new PaymentInputDto()
                                                {
                                                    AccountNumber = paymentEvent.ToAccountNumber,
                                                    Amount = monthlyAmount,
                                                    BankName = paymentEvent.ToBankName,
                                                    BranchCode = paymentEvent.ToBankBranchCode,
                                                    FulfilmentDate = "",
                                                    InitiationDate = paymentEvent.InitiatedDateTime.ToString("G"),
                                                    IsFraud = false,
                                                    ModifiedBy = "api-admin",
                                                    PaymentType = paymentEvent.PaymentType,
                                                });
                                                // subtract from the payment balance
                                                _logger.LogInformation($"Payment has been added to the smart contract in block: {results.BlockNumber}, function sighashes: {results.FunctionSigHashes}, timestamp: {results.BlockHash} on address: {results.ContractAddress}");
                                                
                                                fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                fundingUpdateDto.IsActive = true;
                                                fundingUpdateDto.AccommodationBalance = fundingContract.FoodBalance - monthlyAmount;
                                                fundingUpdateDto.ModifiedBy = "api-admin";
                                                // update the funding
                                                await funding.UpdateFundingAsync(fundingUpdateDto);
                                            }
                                            else
                                            {
                                                _logger.LogError($"Payment event for {student.Name}  for accommodation could not be processed because of insufficient monthly balance of ZAR {fundingContract.FoodBalance}");
                                            }
                                        }
                                        if (fundingContract.LaptopBalance > 0)
                                        {
                                            var monthlyAmount = fundingConditions.LaptopAmount / 10;
                                            var fundingUpdateDto = new FundingUpdateDto();
                                            if (fundingContract.LaptopBalance > monthlyAmount)
                                            {
                                                paymentEvent.Amount = monthlyAmount;
                                                
                                                //publish this event
                                                paymentEvent.PaymentType = "Laptop/Book";
                                                await paymentPublisher.Publish(paymentEvent);
                                                var results = await paymentContract.AddPaymentAsync(new PaymentInputDto()
                                                {
                                                    AccountNumber = paymentEvent.ToAccountNumber,
                                                    Amount = monthlyAmount,
                                                    BankName = paymentEvent.ToBankName,
                                                    BranchCode = paymentEvent.ToBankBranchCode,
                                                    FulfilmentDate = "",
                                                    InitiationDate = paymentEvent.InitiatedDateTime.ToString("G"),
                                                    IsFraud = false,
                                                    ModifiedBy = "api-admin",
                                                    PaymentType = paymentEvent.PaymentType,
                                                });
                                                // subtract from the payment balance
                                                _logger.LogInformation($"Payment has been added to the smart contract in block: {results.BlockNumber}, function sighashes: {results.FunctionSigHashes}, timestamp: {results.BlockHash} on address: {results.ContractAddress}");
                                                fundingUpdateDto.Id = fundingContract.Id.ToString();
                                                fundingUpdateDto.IsActive = true;
                                                fundingUpdateDto.LaptopBalance = fundingContract.LaptopBalance - monthlyAmount;
                                                fundingUpdateDto.ModifiedBy = "api-admin";
                                                // update the funding
                                                await funding.UpdateFundingAsync(fundingUpdateDto);
                                            }
                                            else
                                            {
                                                _logger.LogError($"Payment event for {student.Name}  for accommodation could not be processed because of insufficient monthly balance of ZAR {fundingContract.FoodBalance}");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!allStudents.Any())
                        {
                            _logger.LogError($"Could not find students belonging to {funder.Name} therefore, cannot proceed with payment generation");
                        }
                    }
                }
                else
                {
                    _logger.LogError($"Could not find any user for {funder.Name} and as such cannot retrieve any banking details. make sure user and bank details for the funder are added to the system.");
                }
            }
            _logger.LogInformation($"Payment Date: {funder.PaymentDate} for {funder.Name} is not in the next 48 hours. No further processing required.");
        }
    }
}