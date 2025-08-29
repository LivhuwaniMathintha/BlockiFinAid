using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class UserAccessControl : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonIgnore]
    public Guid UserIdPerformingAction { get; set; }
    [BsonIgnore]
    public string StudentNumber { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public IEnumerable<string> Permissions { get; set; } =  new List<string>();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsAccountConfirmed { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool IsSuccess { get; set; }= false;
    public string Role { get; set; } = string.Empty;
}

public class AccessControlPermissions : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserIdPerformingAction { get; set; }
    [BsonIgnore]
    public string StudentNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    
}