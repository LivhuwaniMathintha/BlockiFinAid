using BlockiFinAid.Services.SmartContracts.Payment;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class PaymentModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string TransactionGroupId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid StudentId { get; set; } 
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid InstitutionServiceId { get; set; }

    public string Funder { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
   
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
    public string StudentNumber { get; set; }
    public  bool IsFraud { get; set; }
}

public class FraudDataModel : PaymentModel
{
    public bool IsFraud { get; set; }
    public bool IsPaymentDuplicate { get; set; }
    public string ProcessingCountry { get; set; } =  string.Empty;
    // so many other fields can be added in here depending on context
}

public class PaymentCheckerModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
 
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserIdPerformingAction { get; set; }
    [BsonIgnore]
    public string StudentNumber { get; set; }

    public DateTime LastPaidDate { get; set; }
 
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid FunderId { get; set; }
}