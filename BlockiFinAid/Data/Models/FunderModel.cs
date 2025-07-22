using BlockiFinAid.Services.SmartContracts.Funder;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class FunderModel : IModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid FunderContractId { get; set; }
    public bool IsChangeConfirmed { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}