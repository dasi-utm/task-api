using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common;

public static class TaskMapper
{
    public static TaskDto ToDto(TaskItem task)
    {
        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.Priority.ToString(),
            task.DueDate,
            task.AssignedToId,
            task.CreatedById,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
