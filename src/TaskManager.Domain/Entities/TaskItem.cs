using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Events;

namespace TaskManager.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskItemStatus Status { get; private set; } = TaskItemStatus.Pending;
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; private set; }
    public Guid? AssignedToId { get; private set; }
    public Guid CreatedById { get; private set; }

    public User? AssignedTo { get; private set; }
    public User? CreatedByUser { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string title, string description, TaskPriority priority, DateTime? dueDate, Guid createdById)
    {
        var task = new TaskItem
        {
            Title = title,
            Description = description,
            Priority = priority,
            DueDate = dueDate,
            CreatedById = createdById,
            Status = TaskItemStatus.Pending,
            CreatedBy = createdById.ToString()
        };

        task.RaiseDomainEvent(new TaskCreatedEvent(task.Id, task.Title, createdById));
        return task;
    }

    public void Update(string title, string description, TaskPriority priority, DateTime? dueDate)
    {
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskUpdatedEvent(Id, Title));
    }

    public void ChangeStatus(TaskItemStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to {newStatus}");

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public void AssignTo(Guid userId)
    {
        AssignedToId = userId;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskAssignedEvent(Id, userId));
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskDeletedEvent(Id));
    }

    private static bool IsValidTransition(TaskItemStatus current, TaskItemStatus next)
    {
        return (current, next) switch
        {
            (TaskItemStatus.Pending, TaskItemStatus.InProgress) => true,
            (TaskItemStatus.Pending, TaskItemStatus.Cancelled) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Completed) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Pending) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Cancelled) => true,
            _ => false
        };
    }
}
