using BlockiFinAid.Services.SmartContracts.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public class UserModel : IModel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
   // [BsonElement("institution_id")]
   [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid InstitutionId { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
   // [BsonElement("funder_contract_id")]
    public Guid FunderContractId { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
   // [BsonElement("bank_account_id")]
    public Guid BankAccountId { get; set; }
   // [BsonElement("student_number")] 
    public string StudentNumber { get; set; } = string.Empty;
   // [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
   // [BsonElement("email")]
    public string Email { get; set; } = string.Empty;
  //  [BsonElement("is_active")]
    public bool IsActive { get; set; }
   // [BsonElement("is_change_confirmed")]
    public bool IsChangeConfirmed { get; set; }
   // [BsonElement("modified_by")]
    public string ModifiedBy { get; set; } = string.Empty;
   // [BsonElement("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
   // [BsonElement("course_name")]
    public string CourseName { get; set; } = string.Empty;
   // [BsonElement("role")]
    public string Role { get; set; } =  string.Empty;
    [BsonIgnore]
    public Guid UserIdPerformingAction { get; set; }
}