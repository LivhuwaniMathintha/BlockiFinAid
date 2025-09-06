using BlockiFinAid.Services.SmartContracts.Funding;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class FundingConditionsModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    public bool IsFullCoverage { get; set; }
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime StartDate { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime EndDate { get; set; }
    public uint TotalAmount { get; set; }
    public uint FoodAmount { get; set; }
    public uint TuitionAmount { get; set; }
    public uint LaptopAmount { get; set; }
    public uint AccommodationAmount { get; set; }
    public bool AccommodationDirectPay { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid DataConfirmedById { get; set; }
    public string ModifiedBy { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public uint AverageMark { get; set; }
    
    [BsonIgnore]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserIdPerformingAction { get; set; }

    [BsonIgnore] public string StudentNumber { get; set; } = string.Empty;
}