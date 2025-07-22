using BlockiFinAid.Services.SmartContracts.BankAccount;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public abstract class BankAccountModel : IModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public Guid Id { get; set; }
    
    [BsonElement("bank_account_id")]
    public string BankAccountNumber { get; set; } = string.Empty;

    [BsonElement("bank_name")] 
    public string BankName { get; set; } = string.Empty;
    
    [BsonElement("bank_branch_code")]
    public string BankBranchCode { get; set; } = string.Empty;
    
    [BsonElement("is_confirmed")]
    public bool IsConfirmed { get; set; }
    
    [BsonElement("student_number")]
    public uint StudentNumber { get; set; }
    
    [BsonElement("data_confirmed_by_id")]
    public Guid DataConfirmedById { get; set; }
    
    [BsonElement("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;

 
}