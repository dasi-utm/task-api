using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<(IReadOnlyList<TaskItem> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        TaskItemStatus? status = null,
        TaskPriority? priority = null,
        Guid? assignedToId = null,
        string? sortBy = null,
        bool sortDescending = false);
}
