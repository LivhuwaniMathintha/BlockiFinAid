using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services.AccessControl;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.Payment;
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
        await Task.Delay(TimeSpan.FromSeconds(3));
        _logger.LogInformation($"Consuming the following user {context.Message.StudentNumber} from the queue: ");
        var userDto = new UserInputDto
        {
            Name = context.Message.Name,
            Email = context.Message.Email,
            StudentNumber = context.Message.StudentNumber,
            BankAccountId = context.Message.BankAccountId.ToString(),
            CourseName = context.Message.CourseName,
            FunderContractId = context.Message.FunderContractId.ToString(),
            IsActive = context.Message.IsActive,
            ModifiedBy = context.Message.ModifiedBy,
            IsChangeConfirmed = context.Message.IsChangeConfirmed,
            InstitutionId = context.Message.InstitutionId.ToString(),
            Role =  context.Message.Role,
            Id = context.Message.Id.ToString(),
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
        await Task.Delay(TimeSpan.FromSeconds(6));
        var accountDto = new BankAccountInputDto()
        {
            BankAccountNumber = context.Message.BankAccountNumber,
            BankBranchCode = context.Message.BankBranchCode,
            BankName = context.Message.BankName,
            DataConfirmedById = context.Message.DataConfirmedById.ToString(),
            StudentNumber =  context.Message.StudentNumber,
            Id = context.Message.Id.ToString(),
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
        await Task.Delay(TimeSpan.FromSeconds(1));
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

public class FundingConditionsDbConsumer : IConsumer<FundingConditionsModel>
{
    private readonly FundingConditionsContract _fundingConditionsContract;
    private readonly ILogger<FundingConditionsDbConsumer> _logger;

    public FundingConditionsDbConsumer(FundingConditionsContract fundingConditionsContract, ILogger<FundingConditionsDbConsumer> logger)
    {
        _fundingConditionsContract = fundingConditionsContract;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<FundingConditionsModel> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(9));
        _logger.LogInformation($"Consuming funding model for student {context.Message.StudentNumber} from the queue: ");
        var item = new FundingConditionInputDto()
        {
            AccommodationAmount = context.Message.AccommodationAmount,
            AccommodationDirectPay = context.Message.AccommodationDirectPay,
            AverageMark = context.Message.AverageMark,
            FoodAmount = context.Message.FoodAmount,
            TuitionAmount = context.Message.TuitionAmount,
            TotalAmount = context.Message.TotalAmount,
            EndDate = context.Message.EndDate.ToString("G"),
            LaptopAmount = context.Message.LaptopAmount,
            IsFullCoverage = context.Message.IsFullCoverage,
            StartDate = context.Message.StartDate.ToString("G"),
            UpdatedAt = context.Message.UpdatedAt.ToString("G"),
            ModifiedBy = context.Message.ModifiedBy,
            
        };

        var results = await _fundingConditionsContract.AddConditionAsync(item);
        _logger.LogInformation($"Adding data from db to smart contract in real-time for funding conditions model complete with status: {results}");
    }
}

public class PaymentDbConsumer : IConsumer<PaymentModel>
{
 
    private readonly ILogger<PaymentDbConsumer> _logger;
    private readonly PaymentContract _paymentContract;

    public PaymentDbConsumer(ILogger<PaymentDbConsumer> logger, PaymentContract contract)
    {
        _paymentContract = contract;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<PaymentModel> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(7));
        _logger.LogInformation($"Consuming payment from the queue ");
        var paymentDto = new PaymentInputDto
        {
            Id = context.Message.Id.ToString(),
            AccountNumber = context.Message.AccountNumber,
            StudentNumber = context.Message.StudentNumber,
            BranchCode = context.Message.BranchCode,
            BankName = context.Message.BankName,
            Amount = context.Message.Amount,
            PaymentType = context.Message.PaymentType,
            Status = context.Message.Status,
            IsFraud = context.Message.IsFraud,
            InitiationDate = context.Message.InitiationDate.ToString("G"),
            FulfilmentDate = context.Message.FulfilmentDate.ToString("G"),
            ModifiedBy = context.Message.ModifiedBy,
        };
        
        var results = await _paymentContract.AddPaymentAsync(paymentDto);
        _logger.LogInformation($"Payment  added to the smart contract with status: {results.BlockHash}");
    }
}

public class FunderDbConsumer : IConsumer<FunderModel>
{
    private readonly FunderContract _funderContract;
    private readonly ILogger<FunderDbConsumer> _logger;

    public FunderDbConsumer(ILogger<FunderDbConsumer> logger,  FunderContract funderContract)
    {
        _funderContract = funderContract;
        _logger = logger;           
    }
    
    public async Task Consume(ConsumeContext<FunderModel> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        _logger.LogInformation($"Consuming funding model for Funder {context.Message.Name} from the queue: ");
        var item = new FunderInputDto
        {
            Id = context.Message.Id.ToString(),
            IsChangeConfirmed = context.Message.IsChangeConfirmed,
            Name = context.Message.Name,
            ModifiedBy = context.Message.ModifiedBy,
            PaymentDate = context.Message.PaymentDate.ToString("G"),
            FunderContractId = context.Message.FunderContractId.ToString(),
        };

        _logger.LogInformation($"Looking for funder: {item.Name} with id: {item.Id} in the smart contract");
        var isFunderOnContract = await _funderContract.GetFunderByNameAsync(item.Name);
        if (isFunderOnContract?.Name == "")
        {
            try
            {
                var results = await _funderContract.AddFunderAsync(item);
                _logger.LogInformation($"Adding data from db to smart contract in real-time for funder model complete with status: {results.BlockHash}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Funder using name {item.Name} is not in the smart contract, however, this funder is using id: {item.Id} and this is the error: {ex}");
            }
        }
        else
        {
            _logger.LogError($"Funder {isFunderOnContract?.Name} from the queue already exists in the smart contract - blockchain");
        }
    }
}