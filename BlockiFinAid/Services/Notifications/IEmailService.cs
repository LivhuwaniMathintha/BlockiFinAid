using BlockiFinAid.Data.Configs;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BlockiFinAid.Services.Notifications;

 public interface IEmailService
 {
     Task SendEmailAsync(string to, string subject, string body, string htmlContent);
 }
 
public class SendGridMailer : IEmailService
{
    private readonly ILogger<SendGridMailer> _logger;
    private readonly SendGridSettings _sendgrid;

    public SendGridMailer(ILogger<SendGridMailer> logger, IOptions<SendGridSettings> sendgrid)
    {
        _logger = logger;
        _sendgrid = sendgrid.Value;
    }
    public async Task SendEmailAsync(string to, string subject, string body, string htmlContent)
    {
        try
        {

            var client = new SendGridClient(_sendgrid.ApiKey);
            var from = new EmailAddress("blockifinaid@gmail.com", "Blocki Fin Aid");
            var toAddress = new EmailAddress(to, "Blocki Fin Aid");
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, body, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Successfully sent an email to user with status code {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            _logger.LogError(e.Message);
        }
    }
}
