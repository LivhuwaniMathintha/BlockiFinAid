using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class BankAccountsHistory : IModel
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    public ICollection<AccountsAddedDetails> BankAccounts { get; set; } = new List<AccountsAddedDetails>();
    [BsonIgnore]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserIdPerformingAction { get; set; }
    [BsonIgnore]
    public string StudentNumber { get; set; }
}

public class AccountsAddedDetails
{
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public bool IsActive { get; set; }
    public string? InactivityReason { get; set; }
}