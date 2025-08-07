using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services.AccessControl;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.User;
using MassTransit;

namespace BlockiFinAid.Services.Messaging;

public class UserDbConsumer : IConsumer<UserModel>
{
    private readonly UserContract _userContract;
    private readonly ILogger<UserDbConsumer> _logger;

    public UserDbConsumer(ILogger<UserDbConsumer> logger, UserContract userContract)
    {
        _userContract = userContract;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<UserModel> context)
    {
        _logger.LogInformation($"Consuming the following user {context.Message.StudentNumber} from the queue: ");
        var userDto = new UserInputDto
        {
            Name = context.Message.Name,
            Email = context.Message.Email,
            StudentNumber = context.Message.StudentNumber,
            BankAccountId = "hjkh",
            CourseName = context.Message.CourseName,
            FunderContractId = "jhkjh",
            IsActive = context.Message.IsActive,
            ModifiedBy = context.Message.ModifiedBy,
            IsChangeConfirmed = context.Message.IsChangeConfirmed,
            InstitutionId = "sdss",
            Role =  context.Message.Role,
        };
        
        var results = await _userContract.AddUserAsync(userDto);
        _logger.LogInformation($"User {context.Message.StudentNumber} added to the smart contract for users with status: {results}");
    }
}

public class BankAccountDbConsumer : IConsumer<BankAccountModel>
{
    private readonly ILogger<BankAccountDbConsumer> _logger;

    private readonly BankAccountContract _bankAccountContract;


    public BankAccountDbConsumer(ILogger<BankAccountDbConsumer> logger, BankAccountContract bankAccountContract)
    {
        _logger = logger;
        _bankAccountContract = bankAccountContract;
    }
    public async Task Consume(ConsumeContext<BankAccountModel> context)
    {
        _logger.LogInformation($"BankAccountNumber: {context.Message.BankAccountNumber} has been added to the database and retrieved in real-time. Queue for Smart contract addition");

        var accountDto = new BankAccountInputDto()
        {
            BankAccountNumber = context.Message.BankAccountNumber,
            BankBranchCode = context.Message.BankBranchCode,
            BankName = context.Message.BankName,
            DataConfirmedById = context.Message.DataConfirmedById.ToString(),
            StudentNumber =  context.Message.StudentNumber
        };
        
        var results = await _bankAccountContract.AddBankAccountAsync(accountDto);
        
        _logger.LogInformation($"Adding data from db to smart contract in real-time for bankAccount model is complete with status: {results}");
    }
}

public class FundingDbConsumer : IConsumer<FundingModel>
{
    private readonly FundingContract _fundingContract;
    private readonly ILogger<FundingDbConsumer> _logger;

    public FundingDbConsumer(ILogger<FundingDbConsumer> logger,  FundingContract fundingContract)
    {
        _fundingContract = fundingContract;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<FundingModel> context)
    {
        _logger.LogInformation($"Consuming funding model for student {context.Message.StudentNumber} from the queue: ");
        var fundingDto = new FundingInputDto
        {
            StudentId = context.Message.StudentId.ToString(),
            FunderContractConditionId = context.Message.FunderContractConditionId.ToString(),
            DataConfirmedById = context.Message.DataConfirmedById.ToString(),
            SignedOn = context.Message.SignedOn.ToString("G"),
            FoodBalance = context.Message.FoodBalance,
            TuitionBalance = context.Message.TuitionBalance,
            LaptopBalance = context.Message.LaptopBalance,
            AccommodationBalance = context.Message.AccommodationBalance,
            ModifiedBy = context.Message.ModifiedBy
        };
        
        var results = await _fundingContract.AddFundingAsync(fundingDto);
        _logger.LogInformation($"Adding data from db to smart contract in real-time for funding model complete with status: {results}");
    }
}

