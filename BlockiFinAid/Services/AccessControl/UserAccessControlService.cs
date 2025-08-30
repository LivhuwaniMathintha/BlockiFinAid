using BlockiFinAid.Data.DTOs;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services.Repository;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace BlockiFinAid.Services.AccessControl;

public class UserAccessControlService
{
    private readonly IPasswordHasher<UserAccessControl> _passwordHasher;
    private readonly IMongoCollection<UserAccessControl> _db;
    private readonly ILogger<UserAccessControlService> _logger;
    private readonly IBaseRepository<UserModel> _userRepository;

    public UserAccessControlService(IPasswordHasher<UserAccessControl> passwordHasher, IMongoDatabase mongoDatabase, ILogger<UserAccessControlService> logger, IBaseRepository<UserModel> userRepository)
    {
        _passwordHasher = passwordHasher;
        _db = mongoDatabase.GetCollection<UserAccessControl>("UserAccess");
        _logger = logger;
        _userRepository = userRepository;
        
    }

    public UserAccessControl? HashPasswordForNewUser(UserAccessControl user, string plainTextPassword)
    {
        _logger.LogInformation("Hashing password for new user: {user}", user);
        try
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, plainTextPassword);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Hashing password for new user: {user}", user);
            return null;
        }
    }

    public bool VerifyPassword(UserAccessControl user, string plainTextPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, plainTextPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }

   
    public async Task<UAMResponse?> RegisterUser(UserAccessControlDto user)
    {
        
        var newUser = new UserAccessControl
        {
            Id = Guid.NewGuid(),
            Name = user.Name,
            Role = user.Role,
            Permissions = user.Permissions,
            IsAccountConfirmed = false,
            Email = user.Email,
            PasswordHash = "",
        };
        
        var hashedUser =  HashPasswordForNewUser(newUser, user.Password);

        try
        {
            if (hashedUser is not null)
            {
                await _db.InsertOneAsync(hashedUser);
                _logger.LogInformation($"User {hashedUser.Name} has been registered");
                return new UAMResponse
                {
                    UserId = newUser.Id,
                    Role = hashedUser.Role,
                    Email = hashedUser.Email,
                    Password = newUser.PasswordHash,
                    Permissions = hashedUser.Permissions,
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while registering user {hashedUser?.Name}: {ex.Message}");
            return null;
        }
        return null;
    }

    public async Task<UserAccessControl?> GetUserByEmail(string email)
    {
        var user = await _db.Find(x => x.Email == email).FirstOrDefaultAsync();

        return user ?? null;
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
            // get student number
            var actualUsers = await _userRepository.GetAllAsync();
            var studentNumber = actualUsers.FirstOrDefault(x => x.Email == user.Email)?.StudentNumber;
            user.StudentNumber = studentNumber;
            return user; // Login successful
        }

        _logger.LogError($"Invalid username or password");
        return null; // Invalid password
    }

    public async Task<UserAccessControl?> GetUserPerformingActionById(Guid modelUserIdPerformingAction)
    {
        var user = await _db.Find(x => x.Id == modelUserIdPerformingAction).FirstOrDefaultAsync();
        return user ?? null;
    }
    
    public async Task<UserAccessControl?> GetUserById(Guid userId)
    {
        var user = await _db.Find(x => x.Id == userId).FirstOrDefaultAsync();
        return user ?? null;
    }
}