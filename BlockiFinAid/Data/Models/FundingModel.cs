using BlockiFinAid.Services.SmartContracts.Funding;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class FundingModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid FunderId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid StudentId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid FunderContractConditionId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
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

    [BsonIgnore]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserIdPerformingAction { get; set; }
    [BsonIgnore]
    public string StudentNumber { get; set; }
}