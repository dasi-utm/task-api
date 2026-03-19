using TaskManager.Domain.Common;

namespace TaskManager.Domain.Events;

public class TaskCreatedEvent : DomainEvent
{
    public Guid TaskId { get; }
    public string Title { get; }
    public Guid CreatedById { get; }

    public TaskCreatedEvent(Guid taskId, string title, Guid createdById)
    {
        TaskId = taskId;
        Title = title;
        CreatedById = createdById;
    }
}
