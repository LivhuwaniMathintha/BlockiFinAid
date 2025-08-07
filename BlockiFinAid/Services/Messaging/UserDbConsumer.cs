using BlockiFinAid.Data.Models;
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

    public BankAccountDbConsumer(ILogger<BankAccountDbConsumer> logger)
    {
        _logger = logger;
    }
    public Task Consume(ConsumeContext<BankAccountModel> context)
    {
        var account = context.Message.BankAccountNumber;
        _logger.LogInformation($"BankAccountNumber: {account} has been added to the database and retrieved in real-time. Queue for Smart contract addition");
        return Task.CompletedTask;
    }
}

