using TaskManager.Domain.Common;

namespace TaskManager.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<DomainEvent> events);
}
