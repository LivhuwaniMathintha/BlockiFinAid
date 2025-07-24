using BlockiFinAid.Services.SmartContracts.BankAccount;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class BankAccountModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string BankAccountNumber { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public string BankBranchCode { get; set; } = string.Empty;
 
    public bool IsConfirmed { get; set; }
    
    public uint StudentNumber { get; set; }
 
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid DataConfirmedById { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


}