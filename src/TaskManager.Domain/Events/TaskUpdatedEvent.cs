using TaskManager.Domain.Common;

namespace TaskManager.Domain.Events;

public class TaskUpdatedEvent : DomainEvent
{
    public Guid TaskId { get; }
    public string Title { get; }

    public TaskUpdatedEvent(Guid taskId, string title)
    {
        TaskId = taskId;
        Title = title;
    }
}
