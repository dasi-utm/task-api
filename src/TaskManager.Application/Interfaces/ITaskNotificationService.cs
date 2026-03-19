using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskNotificationService
{
    Task TaskCreated(TaskDto task);
    Task TaskUpdated(TaskDto task);
    Task TaskDeleted(Guid taskId);
    Task TaskStatusChanged(Guid taskId, string newStatus);
}
