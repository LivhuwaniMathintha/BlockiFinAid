using BlockiFinAid.Data.DTOs;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace BlockiFinAid.Services.AccessControl;

public class UserAccessControlService
{
    private readonly IPasswordHasher<UserAccessControl> _passwordHasher;
    private readonly IMongoCollection<UserAccessControl> _db;
    private readonly ILogger<UserAccessControlService> _logger;

    public UserAccessControlService(IPasswordHasher<UserAccessControl> passwordHasher, IMongoDatabase mongoDatabase, ILogger<UserAccessControlService> logger)
    {
        _passwordHasher = passwordHasher;
        _db = mongoDatabase.GetCollection<UserAccessControl>("UserAccess");
        _logger = logger;
    }

    public UserAccessControl HashPasswordForNewUser(UserAccessControl user, string plainTextPassword)
    {
        user.PasswordHash = _passwordHasher.HashPassword(user, plainTextPassword);
        return user;
    }

    public bool VerifyPassword(UserAccessControl user, string plainTextPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, plainTextPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    public async Task<UserAccessControl?> RegisterUser(UserAccessControlDto user)
    {

        var newUser = new UserAccessControl
        {
            Id = Guid.NewGuid(),
            Name = user.Name,
            Role = user.Role,
            Permissions = user.Permissions,
            IsAccountConfirmed = false,
            Email = user.Email,
            PasswordHash = string.Empty
        };
        
        var hashedUser =  HashPasswordForNewUser(newUser, user.Password);

        try
        {
            await _db.InsertOneAsync(hashedUser);
            _logger.LogInformation($"User {hashedUser.Name} has been registered");
            return _db.Find(x => x.Id == hashedUser.Id).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while registering user {hashedUser.Name}: {ex.Message}");
            return null;
        }
    }
    
    public async Task<UserAccessControl?> LoginUser(string email, string plainTextPassword)
    {
        // 1. Retrieve the user by email from your database
        
        var user = await _db.Find(x => x.Email == email).FirstOrDefaultAsync(); // Assuming you have such a method
        
        if (user == null)
        {
            _logger.LogError($"User {email} not found");
            return null; // User not found
        }

        // 2. Verify the provided password
        if (VerifyPassword(user, plainTextPassword))
        {
            return user; // Login successful
        }

        _logger.LogError($"Invalid username or password");
        return null; // Invalid password
    }
}