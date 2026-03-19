using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Events;

public class TaskStatusChangedEvent : DomainEvent
{
    public Guid TaskId { get; }
    public TaskItemStatus OldStatus { get; }
    public TaskItemStatus NewStatus { get; }

    public TaskStatusChangedEvent(Guid taskId, TaskItemStatus oldStatus, TaskItemStatus newStatus)
    {
        TaskId = taskId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
