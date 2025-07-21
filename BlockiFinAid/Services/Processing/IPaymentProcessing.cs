using BlockiFinAid.Data.Configs;
using BlockiFinAid.Data.QueueEvents;
using BlockiFinAid.Data.Responses;
using BlockiFinAid.Services.Notifications;
using Microsoft.Extensions.Options;

namespace BlockiFinAid.Services.Processing;

public interface IPaymentProcessing
{
    Task<ServiceResponse<string>> ProcessPayment(PaymentCreatedEvent paymentCreatedEvent);
}


public class PaymentProcessingService : IPaymentProcessing
{
    private readonly IEmailService _emailService;
    private readonly SendGridSettings _addresses;

    public PaymentProcessingService(IEmailService emailService, IOptions<SendGridSettings> sendGridSettings)
    {
        _emailService = emailService;
        _addresses = sendGridSettings.Value;
    }
    public async Task<ServiceResponse<string>> ProcessPayment(PaymentCreatedEvent paymentCreatedEvent)
    {
        // Add Stripe / warawara
        try
        {
            // add data to a database for record keeping
            
            return new ServiceResponse<string>()
            {
                Data = "",
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            await _emailService.SendEmailAsync(_addresses.ServiceEmailAddress ?? "zitoesn@gmail.com", "Payment Error",  ex.Message, "");
            return new ServiceResponse<string>
            {
                Data = null,
                Error = new List<string?>() { $"{ex.Message}" }
            };
        }
    }
}