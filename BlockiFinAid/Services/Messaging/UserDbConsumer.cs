using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services.AccessControl;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using MassTransit;

namespace BlockiFinAid.Services.Messaging;

public class UserDbConsumer : IConsumer<UserModel>
{
    public Task Consume(ConsumeContext<UserModel> context)
    {
        throw new NotImplementedException();
    }
}

public class BankAccountDbConsumer : IConsumer<BankAccountModel>
{
    private readonly ILogger<BankAccountDbConsumer> _logger;

    private readonly BankAccountContract _bankAccountContract;
    private readonly UserAccessControlService _userRepository;

    public BankAccountDbConsumer(ILogger<BankAccountDbConsumer> logger, UserAccessControlService userRepository, BankAccountContract bankAccountContract)
    {
        _logger = logger;
        _userRepository = userRepository;
        _bankAccountContract = bankAccountContract;
    }
    public async Task Consume(ConsumeContext<BankAccountModel> context)
    {
        var user = await _userRepository.GetUserById(context.Message.UserId);  
        _logger.LogInformation($"BankAccountNumber: {context.Message.StudentNumber} has been added to the database and retrieved in real-time. Queue for Smart contract addition");

        var accountDto = new BankAccountInputDto()
        {
            BankAccountNumber = context.Message.BankAccountNumber,
            BankBranchCode = context.Message.BankBranchCode,
            BankName = context.Message.BankName,
            DataConfirmedById = context.Message.DataConfirmedById.ToString(),
            StudentNumber =  context.Message.StudentNumber
        };
        
        var results = await _bankAccountContract.AddBankAccountAsync(accountDto);
        
        _logger.LogInformation($"Adding data from db to smart contract in real-time complete: {results}");
    }
}

