using MassTransit;

namespace BlockiFinAid.Services.Messaging;

public interface IPublisher
{
    Task Publish<T>(T message);
}

public class MassTransitPublisher : IPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<MassTransitPublisher> _logger;

    public MassTransitPublisher(IBus bus, ILogger<MassTransitPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }
    public async Task Publish<T>(T message)
    {
        if (message != null)
        {
            await _bus.Publish(message);
            _logger.LogInformation($"Published message of type: {typeof(T).Name}");
        }
        
        _logger.LogInformation($"Could not publish message of type: {typeof(T).Name} due to missing message");
        
    }
}