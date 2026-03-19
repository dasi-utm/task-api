using TaskManager.Domain.Common;

namespace TaskManager.Domain.Events;

public class TaskDeletedEvent : DomainEvent
{
    public Guid TaskId { get; }

    public TaskDeletedEvent(Guid taskId)
    {
        TaskId = taskId;
    }
}
