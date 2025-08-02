using System.Text.Json;
using BlockiFinAid.Data.Configs;
using BlockiFinAid.Data.DTOs;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.AccessControl;
using BlockiFinAid.Services.Repository;
using Microsoft.Extensions.Options;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace BlockiFinAid.Helpers;

public class DatabaseSeeder
{
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly MongoDbSettings _mongoDbSettings;
    private readonly UserAccessControlService _uam;

    public DatabaseSeeder(ILogger<DatabaseSeeder> logger, IWebHostEnvironment env, IOptions<MongoDbSettings> settings, UserAccessControlService userAccessControlService)
    {
        _logger = logger;
        _env = env;
        _mongoDbSettings = settings.Value;
        _uam = userAccessControlService;
    }

    public async Task SeedDataAsync()
    {
        _logger.LogInformation("Seeding users...");
        
        // seed users including admin
        await SeedUsersFromJsonAsync();
    }

    private async Task SeedUsersFromJsonAsync()
    {
        var seedFilePath = Path.Combine(_env.ContentRootPath, _mongoDbSettings.UsersJsonFilePath);

        if (!File.Exists(seedFilePath))
        {
            _logger.LogInformation($"Users seed file not found at {seedFilePath}. Skipping user seeding");
            return;
        }

        try
        {
            _logger.LogInformation($"Reading users from Json file {seedFilePath}");
            var jsonContent = await File.ReadAllTextAsync(seedFilePath);
            var usersToSeed = JsonSerializer.Deserialize<List<UserAccessControlDto>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (usersToSeed == null || !usersToSeed.Any())
            {
                _logger.LogInformation($"No users found to seed. Skipping user seeding");
                return;
            }

            _logger.LogInformation($"Found {usersToSeed.Count} users in the JSON file. Starting to seed...");
            foreach (var user in usersToSeed)
            {
                // check if a specific user (e.g by student number) already exists
                // This prevents duplicate data on every startup

                var existingUser = await _uam.GetUserByEmail(user.Email);
                if (existingUser is null)
                {
                    _logger.LogInformation($"User with email {user.Email} does not exist...");
                    var newUser = new UserAccessControlDto
                    {
                        Email = user.Email,
                        Role = user.Role,
                        Permissions = user.Permissions,
                        Name = user.Name,
                        Password = UAMHelper.PasswordGenerator()
                    };
                    
                    _logger.LogInformation($"Processing user {user.Email}...");

                    var result = await _uam.RegisterUser(newUser);

                    if (result is not null)
                    {
                        _logger.LogInformation($"User {result.Email} has been registered with role {result.Role} and a password of {newUser.Password} and Id of {result.UserId}");
                    }
                }
                else
                {
                    _logger.LogInformation($"User with email {user.Email} already exists...");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while seeding users");
        }
    }
}