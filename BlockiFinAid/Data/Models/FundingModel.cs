using BlockiFinAid.Services.SmartContracts.Funding;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public abstract class FundingModel : IModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public Guid Id { get; set; }

    public Guid FunderId { get; set; }
    public Guid StudentId { get; set; }
    public Guid FunderContractConditionId { get; set; }
    public Guid DataConfirmedById { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime SignedOn { get; set; } 

    public bool IsActive { get; set; }
    public uint FoodBalance { get; set; }
    public uint TuitionBalance { get; set; }
    public uint LaptopBalance { get; set; }
    public uint AccommodationBalance { get; set; }

    public string ModifiedBy { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } 
}