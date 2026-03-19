using TaskManager.Domain.Common;

namespace TaskManager.Domain.Events;

public class TaskAssignedEvent : DomainEvent
{
    public Guid TaskId { get; }
    public Guid AssignedToId { get; }

    public TaskAssignedEvent(Guid taskId, Guid assignedToId)
    {
        TaskId = taskId;
        AssignedToId = assignedToId;
    }
}
