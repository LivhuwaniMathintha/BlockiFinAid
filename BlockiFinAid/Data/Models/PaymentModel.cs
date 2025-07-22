using BlockiFinAid.Services.SmartContracts.Payment;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class PaymentModel : IModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public uint Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public bool IsFraud { get; set; }
    public DateTime InitiationDate { get; set; }
    public DateTime FulfilmentDate { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
}