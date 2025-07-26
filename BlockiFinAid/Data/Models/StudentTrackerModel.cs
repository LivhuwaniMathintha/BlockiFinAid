using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class StudentTrackerModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string StudentNumber { get; set; } = string.Empty;
    public bool IsAddedToContract { get; set; } = false;
    [BsonIgnore]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserIdPerformingAction { get; set; }
}