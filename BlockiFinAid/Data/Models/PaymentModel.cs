using BlockiFinAid.Services.SmartContracts.Payment;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class PaymentModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
   
    public string AccountNumber { get; set; } = string.Empty;
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid StudentId { get; set; } 
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid InstitutionServiceId { get; set; }
   
    public string BranchCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public uint Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public DateTime InitiationDate { get; set; }
    public DateTime FulfilmentDate { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    [BsonIgnore]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserIdPerformingAction { get; set; }
}

public class FraudDataModel : PaymentModel
{
    public bool IsFraud { get; set; }
    public bool IsPaymentDuplicate { get; set; }
    public string ProcessingCountry { get; set; } =  string.Empty;
    // so many other fields can be added in here depending on context
}