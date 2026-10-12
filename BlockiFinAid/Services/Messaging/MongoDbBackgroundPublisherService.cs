using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Observability;
using BlockiFinAid.Services.Repository;

namespace BlockiFinAid.Services.Messaging;

public class MongoDbBackgroundPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MongoDbBackgroundPublisherService> _logger;
    private readonly OperationTimingTracker _timingTracker;

    public MongoDbBackgroundPublisherService(IServiceScopeFactory scopeFactory, ILogger<MongoDbBackgroundPublisherService> logger, OperationTimingTracker timingTracker)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timingTracker = timingTracker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MongoDB Database Background Service for Data Streaming is starting.");

        stoppingToken.Register(() => _logger.LogInformation("MongoDB Database Background Service for Data Streaming is stopping."));

        // Create a separate task for each change stream to run concurrently.
        var userTask = Task.Run(() => ProcessStreamAsync<UserModel>(stoppingToken), stoppingToken);
        var bankAccountTask = Task.Run(() => ProcessStreamAsync<BankAccountModel>(stoppingToken), stoppingToken);
        var fundingTask = Task.Run(() => ProcessStreamAsync<FundingModel>(stoppingToken), stoppingToken);
        var fundingConditionsTask = Task.Run(() => ProcessStreamAsync<FundingConditionsModel>(stoppingToken), stoppingToken);
        var funderTask = Task.Run(() => ProcessStreamAsync<FunderModel>(stoppingToken), stoppingToken);
        var paymentTask = Task.Run(() => ProcessStreamAsync<PaymentModel>(stoppingToken), stoppingToken);
        // Wait for all tasks to complete, which will only happen when the service is stopped.
        await Task.WhenAll(userTask, bankAccountTask, fundingTask, fundingConditionsTask, funderTask,  paymentTask);

        _logger.LogInformation("MongoDB Database Background Service for Data Streaming has stopped.");
    }

    private async Task ProcessStreamAsync<T>(CancellationToken stoppingToken) where T : class, IModel
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<T>>();

        _logger.LogInformation($"Starting to watch for new inserts in {typeof(T).Name}.");

        // The 'await foreach' loop will consume the IAsyncEnumerable stream continuously.
        await foreach (var model in repository.StreamInserts().WithCancellation(stoppingToken))
        {
            _logger.LogInformation($"New {typeof(T).Name} entry found. Publishing to Rabbit MQ.");
            await Publish(model, stoppingToken);
        }
    }

    private async Task Publish<T>(T model, CancellationToken stoppingToken) where T : class
    {
        if (model is PaymentModel { DataSource: not DataSources.Live } payment)
        {
            _logger.LogInformation(
                "Skipping RabbitMQ publication for {DataSource} payment {PaymentId}.",
                payment.DataSource,
                payment.Id);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var publishStartedAt = DateTime.UtcNow;
        var modelName = typeof(T).Name;
        try
        {
            _timingTracker.RecordQueuePublish(modelName, publishStartedAt);
            await publisher.Publish(model);
            var elapsedMs = (long)(DateTime.UtcNow - publishStartedAt).TotalMilliseconds;
            _timingTracker.RecordStep("mongo_stream", modelName, "publish_to_rabbit", elapsedMs, modelName);
            _logger.LogInformation($"Publishing to Rabbit MQ has completed for {modelName} in {elapsedMs}ms");
        }
        catch (Exception ex)
        {
            var elapsedMs = (long)(DateTime.UtcNow - publishStartedAt).TotalMilliseconds;
            _timingTracker.RecordStep("mongo_stream", modelName, "publish_to_rabbit", elapsedMs, modelName, "error");
            _logger.LogError(ex, ex.Message);
        }
    }
}