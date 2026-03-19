using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<TaskItem> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        TaskItemStatus? status = null,
        TaskPriority? priority = null,
        Guid? assignedToId = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        var query = _dbSet.AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (assignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == assignedToId.Value);

        var total = await query.CountAsync();

        query = sortBy?.ToLowerInvariant() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "duedate" => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "status" => sortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "createdat" => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}
