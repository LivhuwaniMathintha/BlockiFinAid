using System.Text;
using System.Text.Json;
using BlockiFinAid.Data.Models;
using MongoDB.Driver;

namespace BlockiFinAid.Services.Repository;

public interface IUserRepository
{
    Task<Guid> AddUser(UserModel user);
    Task<UserModel?> GetUser(Guid userId);
    Task<bool> UpdateUser(UserModel user);
    Task<IEnumerable<UserModel>> GetAllUsers();
    Task<UserModel?> StartStreamingAsync();
}

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserModel> _db;

    public UserRepository(IMongoDatabase database)
    {
        _db = database.GetCollection<UserModel>("users");
    }
    public async Task<Guid> AddUser(UserModel user)
    {
        await _db.InsertOneAsync(user);
        return user.Id;
    }

    public async Task<UserModel?> GetUser(Guid userId)
    {
        return await _db.Find(x => x.Id == userId).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateUser(UserModel user)
    {
        try
        {
            await _db.ReplaceOneAsync(x => x.Id == user.Id, user);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<IEnumerable<UserModel>> GetAllUsers()
    {
        return await _db.Find(x => true).ToListAsync();
    }

    public async Task<UserModel?> StartStreamingAsync()
    {
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<UserModel>>()
            .Match(change => change.OperationType == ChangeStreamOperationType.Delete);


        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        };
        
        using var cursor = await _db.WatchAsync(pipeline, options);

        foreach (var change in await cursor.ToListAsync())
        {
            var user = change.FullDocument;
            return user;
        }

        return null;
    }
}