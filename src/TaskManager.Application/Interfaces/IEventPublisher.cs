namespace TaskManager.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(string routingKey, object message);
}
