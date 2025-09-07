using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Payment;
using MassTransit;

namespace BlockiFinAid.Services.Messaging;

public class PaymentEventConsumer : IConsumer<PaymentEvent>
{
    private readonly ILogger<PaymentEventConsumer> _logger;
    private readonly IPublisher _publisher;
    private readonly PaymentContract _paymentContract;

    public PaymentEventConsumer(ILogger<PaymentEventConsumer> logger, FunderContract funderContract, IPublisher publisher, PaymentContract paymentContract)
    {
        _logger = logger;
        _publisher = publisher;
        _paymentContract = paymentContract;
    }

    public async Task Consume(ConsumeContext<PaymentEvent> context)
    {
        PaymentEvent paymentEvent = new PaymentEvent
        {
            FromAccountNumber = context.Message.FromAccountNumber,
            ToAccountNumber = context.Message.ToAccountNumber,
            FromBankBranchCode = context.Message.FromBankBranchCode,
            FromBankName = context.Message.FromBankName,
            ToBankBranchCode = context.Message.ToBankBranchCode,
            ToBankName = context.Message.ToBankName,
            Amount = context.Message.Amount,
            IsFraud = context.Message.IsFraud,
            PaymentType = context.Message.PaymentType,
            IsPaid = context.Message.IsPaid,
            FulfillmentDate = context.Message.FulfillmentDate,
            InitiatedDateTime = context.Message.InitiatedDateTime,
        };
        
        
        try {
            paymentEvent.Status = "processing";
            _logger.LogInformation(
                $"Status: {paymentEvent.Status}: {paymentEvent.PaymentType} payment from {paymentEvent.FromAccountNumber} to {paymentEvent.ToAccountNumber} is being processed");

            paymentEvent.Status = "fulfilled";
            
            // get this payment from the contract and update it
            await _paymentContract.AddPaymentAsync(new PaymentInputDto
            {
                AccountNumber = paymentEvent.ToAccountNumber,
                Amount = (uint)paymentEvent.Amount,
                BankName = paymentEvent.ToBankName,
                BranchCode = paymentEvent.ToBankBranchCode,
                FulfilmentDate = DateTime.UtcNow.ToString("G"),
                InitiationDate = paymentEvent.InitiatedDateTime.ToString("G"),
                IsFraud = false,
                ModifiedBy = "api-admin",
                PaymentType = paymentEvent.PaymentType,
            });
        }
        catch (Exception ex) {
            _logger.LogError(
                $"An error occured while processing payment: {ex.Message}. This payment has been added to the paymentEvent queue with status of [FAIL]");
            
            paymentEvent.Status = "failed";
            await _publisher.Publish(paymentEvent);
        }
    }
}