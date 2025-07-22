using BlockiFinAid.Data.Models;
using MongoDB.Driver;

namespace BlockiFinAid.Services.Repository;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IModel
{
    private readonly IMongoCollection<TEntity> _db;


    public BaseRepository(IMongoDatabase db, string collectionName)
    {
        _db = db.GetCollection<TEntity>(collectionName);
    }
    public async Task<object?> CreateAsync(TEntity model)
    {
        try
        {
            await _db.InsertOneAsync(model);
            return model;
        }
        catch (MongoWriteException ex)
        {
            return null;
        }
    }

    public async Task<bool> UpdateAsync(TEntity model)
    {
        try
        {
            await _db.ReplaceOneAsync(x => x.Id == model.Id, model);
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

    public async Task<object?> StreamInserts()
    {
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TEntity>>()
            .Match(change => change.OperationType == ChangeStreamOperationType.Insert);


        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        };
        
        using var cursor =  await _db.WatchAsync(pipeline, options);

        foreach (var change in await cursor.ToListAsync())
        {
            var user = change.FullDocument;
            return user;
        }

        return null;
    }
}