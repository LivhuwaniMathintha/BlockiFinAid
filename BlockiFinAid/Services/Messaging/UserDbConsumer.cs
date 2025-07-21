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
    public Task Consume(ConsumeContext<BankAccountModel> context)
    {
        throw new NotImplementedException();
    }
}

