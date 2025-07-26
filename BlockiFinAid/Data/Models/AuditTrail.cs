namespace BlockiFinAid.Data.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single audit trail entry in a MongoDB collection.
/// This class captures changes to documents, including their state before and after an operation.
/// </summary>
public class AuditTrail
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit trail entry.
    /// This will be mapped to MongoDB's '_id' field.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)] // Changed to store Guid as a string in MongoDB
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the audit event occurred.
    /// </summary>
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user or system that performed the action.
    /// </summary>
    [BsonElement("userId")]
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the type of action performed (e.g., "Insert", "Update", "Delete").
    /// </summary>
    [BsonElement("actionType")]
    public string ActionType { get; set; }

    /// <summary>
    /// Gets or sets the name of the MongoDB collection where the audited document resides.
    /// </summary>
    [BsonElement("collectionName")]
    public string CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the ID of the document that was affected by the action.
    /// This should correspond to the '_id' of the original document in its collection.
    /// </summary>
    [BsonElement("documentId")]
    [BsonRepresentation(BsonType.String)] // Changed to store Guid as a string in MongoDB
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the state of the document *before* the action.
    /// For 'Insert' operations, this will typically be null.
    /// For 'Update' and 'Delete' operations, it will contain the document's state prior to the change.
    /// Stored as a BsonDocument to allow for flexible schema capture.
    /// </summary>
    [BsonElement("preImage")]
    [BsonIgnoreIfNull] // Don't store if null (e.g., for inserts)
    public BsonDocument? PreImage { get; set; }

    /// <summary>
    /// Gets or sets the state of the document *after* the action.
    /// For 'Delete' operations, this will typically be null.
    /// For 'Insert' and 'Update' operations, it will contain the document's state after the change.
    /// Stored as a BsonDocument to allow for flexible schema capture.
    /// </summary>
    [BsonElement("postImage")]
    [BsonIgnoreIfNull] // Don't store if null (e.g., for deletes)
    public BsonDocument PostImage { get; set; }

 
    // [BsonElement("changes")]
    // [BsonIgnoreIfNull] // Don't store if null (e.g., for inserts/deletes, or if no specific changes tracked)
    // public List<ChangeDetail> Changes { get; set; }

    /// <summary>
    /// Represents a detailed change to a specific field within a document.
    /// </summary>
    public class ChangeDetail
    {
        /// <summary>
        /// Gets or sets the name of the field that was changed.
        /// </summary>
        [BsonElement("fieldName")]
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the old value of the field.
        /// Stored as a BsonValue to handle various data types.
        /// </summary>
        [BsonElement("oldValue")]
        public BsonValue OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value of the field.
        /// Stored as a BsonValue to handle various data types.
        /// </summary>
        [BsonElement("newValue")]
        public BsonValue NewValue { get; set; }
    }
}
