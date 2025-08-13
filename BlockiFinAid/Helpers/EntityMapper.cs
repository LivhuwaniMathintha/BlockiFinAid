using BlockiFinAid.Data.Models;

namespace BlockiFinAid.Helpers;

public static class EntityMapper
{

    public static PaymentResponse ToPaymentResponse(this PaymentModel payment) =>
        new PaymentResponse
        {
            Status = payment.Status,
            Amount = payment.Amount,
            Funder = payment.Funder,
            Time = payment.InitiationDate.ToString("G"),
            IsFraud = payment.IsFraud,
            EndTime = payment.FulfilmentDate.ToString("G"),
        };
}


public class PaymentResponse
{
    public string Status { get; set; }  = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsFraud { get; set; }
    public string Funder { get; set; } = string.Empty;
    
    
}