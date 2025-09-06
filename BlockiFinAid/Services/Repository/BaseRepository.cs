using BlockiFinAid.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BlockiFinAid.Services.Repository;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IModel
{
    private readonly IMongoCollection<TEntity> _db;
    private readonly IMongoCollection<AuditTrail> _auditTrail;
    private readonly string _collectionName;
    private readonly ILogger<BaseRepository<TEntity>> _logger;


    public BaseRepository(IMongoDatabase db, string collectionName, ILogger<BaseRepository<TEntity>> logger)
    {
        _db = db.GetCollection<TEntity>(collectionName);
        _collectionName = collectionName;
        _auditTrail = db.GetCollection<AuditTrail>("audit_trail");
        _logger = logger;
    }
    public async Task<object?> CreateAsync(TEntity model, string userPerformingAction)
    {
        try
        {
            if (model.Id == Guid.Empty)
                model.Id = Guid.NewGuid();

            await _db.InsertOneAsync(model);
            _logger.LogInformation($"Created {typeof(TEntity).Name} with id {model.Id} in the collection {_collectionName}");
            // create audit trail entry for insert
            var auditEntry = new AuditTrail
            {
                Timestamp = DateTime.UtcNow,
                UserId = userPerformingAction,
                ActionType = "Insert",
                CollectionName = _collectionName,
                DocumentId = model.Id,
                PreImage = null,
                PostImage = model.ToBsonDocument()

            };

            await _auditTrail.InsertOneAsync(auditEntry);
            _logger.LogInformation($"Created audit trail with id {auditEntry.Id}");
            return await _db.Find(x => x.Id == model.Id).FirstOrDefaultAsync();
        }
        catch (MongoWriteException ex)
        {
            _logger.LogError($"Error creating document {ex.Message}");
            return ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An unexpected error occured creating document {ex.Message}");
            return ex.Message;
        }
    }

    public async Task<bool> UpdateAsync(TEntity model, string userPerformingAction)
    {
        try
        {
            // Get the current (pre-image) state of the document
            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, model.Id);
            var existingDocument = await _db.Find(filter).FirstOrDefaultAsync();

            if (existingDocument == null)
            {
                _logger.LogInformation($"No records found for {typeof(TEntity).Name} with id {model.Id}");
                return false;
            }
            
            await _db.ReplaceOneAsync(filter, model);
            
            // create audit trail entry for update
            var auditEntry = new AuditTrail
            {
                Timestamp = DateTime.UtcNow,
                UserId = userPerformingAction,
                ActionType = "Update",
                CollectionName = _collectionName,
                DocumentId = model.Id,
                PreImage = existingDocument.ToBsonDocument(),
                PostImage = model.ToBsonDocument()
            };
            
            await _auditTrail.InsertOneAsync(auditEntry);
            _logger.LogInformation($"added audit trail with id {auditEntry.Id} for this entry");
            return true;

        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<TEntity> GetByIdAsync(Guid id)
    {
        return await _db.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _db.Find(x => true).ToListAsync();
    }

    public async Task<TEntity?> FindByStudentNumberAsync(string studentNumber)
    {
        var result = await _db.Find(x => x.StudentNumber == studentNumber).FirstOrDefaultAsync();
        return result;
    }

    public async IAsyncEnumerable<TEntity> StreamInserts(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TEntity>>()
            .Match(change => change.OperationType == ChangeStreamOperationType.Insert);

        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        };

        // Note: Don't use `using var` here because we don't want to dispose the cursor immediately.
        var cursor = await _db.WatchAsync(pipeline, options, cancellationToken);

        // This loop will run forever, yielding new documents as they are inserted.
        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var change in cursor.Current)
            {
                _logger.LogInformation("Found a change in the stream.");
                yield return change.FullDocument;
            }
        }
    }

}