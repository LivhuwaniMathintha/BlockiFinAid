using BlockiFinAid.Helpers;

namespace BlockiFinAid.Services.Messaging;

public class MongoSeederBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MongoSeederBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       var scope = _scopeFactory.CreateScope();
       var seederService = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

       await seederService.SeedDataAsync();
    }
}