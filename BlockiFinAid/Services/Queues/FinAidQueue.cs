using BlockiFinAid.Services.Notifications;
using BlockiFinAid.Services.Processing;
using BlockiFinAid.Services.SmartContracts.Funding;
using MassTransit;
using PaymentCreatedEvent = BlockiFinAid.Data.QueueEvents.PaymentCreatedEvent;

namespace BlockiFinAid.Services.Queues;

public class FinAidQueuePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IEmailService _emailService;

    public FinAidQueuePublisher(IPublishEndpoint endpoint, IEmailService emailService)
    {
        _publishEndpoint =  endpoint;
        _emailService = emailService;
    }

    public async Task PublishPaymentCreated(PaymentCreatedEvent? paymentCreatedEvent)
    {
        if (paymentCreatedEvent == null)
        {
            var htmlContent =
                "<p>Good day,</p><p>Kindly note that there was an error in the FinAidQueue Publisher, there was no valid payment passed through the Publish method.</p></br><p>Regards,</p><p>FinAid Team</p>";
            await _emailService.SendEmailAsync("zitoesn@gmail.com", "Payment Created Event Null", "", htmlContent);
        }

        else
        {
            await _publishEndpoint.Publish(paymentCreatedEvent);
        }
    }
}

public class FinAidQueueConsumer : IConsumer<PaymentCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IPaymentProcessing _paymentProcessing;

    public FinAidQueueConsumer(IEmailService emailService, IPaymentProcessing  paymentProcessing)
    {
        _emailService = emailService;
        _paymentProcessing = paymentProcessing;
    }
    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        var payment = new PaymentCreatedEvent{
            Id = context.Message.Id,
            StudentEmail = context.Message.StudentEmail,
            StudentName = context.Message.StudentName,
            UserId = context.Message.UserId,
            StudentBankAccountNumber = context.Message.StudentBankAccountNumber,
            Amount = context.Message.Amount,
            PaymentType = context.Message.PaymentType,
            StudentBankName = context.Message.StudentBankName,
            StudentBranchCode = context.Message.StudentBranchCode,
            OrgBankAccountNumber = context.Message.OrgBankAccountNumber,
            OrgBankName = context.Message.OrgBankName,
            OrgBankBranchCode = context.Message.OrgBankBranchCode,
            InitiatedDate = context.Message.InitiatedDate,
            IsPaid = context.Message.IsPaid,
            PaymentStatus = context.Message.PaymentStatus
        };
        
        // Process the payment event here
        try
        {
            var results = await _paymentProcessing.ProcessPayment(payment);

            if (!results.IsSuccess)
            {
                Console.WriteLine("Payment Processing Failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }
}