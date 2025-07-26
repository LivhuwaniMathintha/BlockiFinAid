using MassTransit;

namespace BlockiFinAid.Services.Messaging;

public interface IPublisher
{
    Task Publish<T>(T message);
}

public class MassTransitPublisher(IBus bus, ILogger<MassTransitPublisher> logger) : IPublisher
{
    public async Task Publish<T>(T message)
    {
        if (message != null)
        {
            await bus.Publish(message);
            logger.LogInformation($"Published message of type: {typeof(T).Name}");
        }
        else
        {
            logger.LogInformation($"Could not publish message of type: {typeof(T).Name} due to missing message");
        }
    }
}