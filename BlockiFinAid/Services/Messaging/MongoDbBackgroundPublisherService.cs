using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Repository;

namespace BlockiFinAid.Services.Messaging;

public class MongoDbBackgroundPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MongoDbBackgroundPublisherService> _logger;
    private readonly IBaseRepository<UserModel> _userRepository;
    private readonly IBaseRepository<FundingModel> _fundingRepository;
    private readonly IBaseRepository<BankAccountModel> _bankAccountRepository;
    private readonly IBaseRepository<FundingConditionsModel> _fundingConditionsRepository;


    public MongoDbBackgroundPublisherService(IServiceScopeFactory scopeFactory, 
        ILogger<MongoDbBackgroundPublisherService> logger, 
        IBaseRepository<UserModel> userRepository,
        IBaseRepository<BankAccountModel> bankAccountRepository,
        IBaseRepository<FundingModel> fundingRepository,
        IBaseRepository<FundingConditionsModel> fundingConditionsRepository)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _userRepository = userRepository;
        _bankAccountRepository = bankAccountRepository;
        _fundingRepository = fundingRepository;
        _fundingConditionsRepository = fundingConditionsRepository;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service is starting.");
        
        stoppingToken.Register(() => _logger.LogInformation("Background Service is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background Service is working.");


            var user = await _userRepository.StreamInserts();
            if (user != null)
            {
                _logger.LogInformation("MongoDB User Repository Streaming has started and has found a new entry. Publishing to Rabbit MQ has started");
                await Publish(user, stoppingToken);
            } 
           
         
            var bankAccount = await _bankAccountRepository.StreamInserts();
            if (bankAccount != null)
            {
                _logger.LogInformation("MongoDB BankAccount Repository Streaming has started and has found a new entry. Publishing to Rabbit MQ has started");
                await Publish(bankAccount, stoppingToken);
                
            }
            
            var funding = await _fundingRepository.StreamInserts();
            if (funding != null)
            {
                _logger.LogInformation("MongoDB BankAccount Repository Streaming has started and has found a new entry. Publishing to Rabbit MQ has started");
                await Publish(funding, stoppingToken);
                
            }
            
            var fundingConditions = await _fundingConditionsRepository.StreamInserts();
            if (funding != null)
            {
                _logger.LogInformation("MongoDB BankAccount Repository Streaming has started and has found a new entry. Publishing to Rabbit MQ has started");
                await Publish(fundingConditions, stoppingToken);
                
            }
           
        }
        _logger.LogInformation("Background Service is stopping.");
    }

    private async Task Publish<T>(T model, CancellationToken stoppingToken) where T : class
    {
        // create a new scope for each iteration to resolve scoped services (like IPublisher)
        using var scope = _scopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        try
        {
            await publisher.Publish(model);
            _logger.LogInformation($"Publishing to Rabbit MQ has completed for {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
            
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }
}