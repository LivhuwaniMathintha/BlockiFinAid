using MongoDB.Bson.Serialization.Attributes;

namespace BlockiFinAid.Data.Models;

public interface IModel
{ 
    Guid Id { get; set; }
    public Guid UserIdPerformingAction { get; set; }
    
    public string StudentNumber { get; set; }
}