using MassTransit;

namespace BlockiFinAid.Services.Messaging;

public interface IPublisher
{
    Task Publish<T>(T message);
}

public class MassTransitPublisher : IPublisher
{
    private readonly IBus _bus;

    public MassTransitPublisher(IBus bus)
    {
        _bus = bus;
    }
    public async Task Publish<T>(T message)
    {
        if (message != null)
        {
            await _bus.Publish(message);
            Console.WriteLine($"Published message of type: {typeof(T).Name}");
        }
        
    }
}