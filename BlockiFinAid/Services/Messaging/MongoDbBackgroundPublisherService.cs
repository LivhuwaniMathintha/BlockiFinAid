using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Repository;

namespace BlockiFinAid.Services.Messaging;

public class MongoDbBackgroundPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MongoDbBackgroundPublisherService> _logger;
    private readonly IUserRepository _userRepository;

    public MongoDbBackgroundPublisherService(IServiceScopeFactory scopeFactory, ILogger<MongoDbBackgroundPublisherService> logger, IUserRepository userRepository)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _userRepository = userRepository;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service is starting.");
        
        stoppingToken.Register(() => _logger.LogInformation("Background Service is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background Service is working.");
            
            
            var user = await _userRepository.StartStreamingAsync();
            if (user == null) 
                continue;
            _logger.LogInformation("MongoDB User Repository Streaming has started and has found a new entry. Publishing to Rabbit MQ has started");
            await Publish(user, stoppingToken);
            _logger.LogInformation("Publishing to Rabbit MQ has completed for User Repository.");
            
            // Add bank account repository streaming here
            // _logger.LogInformation("MongoDB BankAccount Repository Streaming has started and has found a new entry. Publishing to Rabbit MQ has started");
            // await Publish(user, stoppingToken);
            // _logger.LogInformation("Publishing to Rabbit MQ has completed for BankAccount Repository.");
        }
        _logger.LogInformation("Background Service is stopping.");
    }

    public async Task Publish<T>(T model, CancellationToken stoppingToken) where T : class
    {
        // create a new scope for each iteration to resolve scoped services (like IPublisher)
        using var scope = _scopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        try
        {
            await publisher.Publish(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
            
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
    }
}