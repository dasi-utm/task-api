using Microsoft.EntityFrameworkCore;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Services;

public class TaskStatisticsService : ITaskStatisticsService
{
    private readonly AppDbContext _context;

    public TaskStatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskStatisticsDto> GetOverallStatisticsAsync()
    {
        var tasks = _context.Tasks;

        var total = await tasks.CountAsync();

        var byStatus = await tasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var byPriority = await tasks
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count);

        var overdueCount = await tasks
            .CountAsync(t => t.DueDate.HasValue
                && t.DueDate < DateTime.UtcNow
                && t.Status != TaskItemStatus.Completed
                && t.Status != TaskItemStatus.Cancelled);

        return new TaskStatisticsDto(total, byStatus, byPriority, overdueCount);
    }

    public async Task<IReadOnlyList<UserStatisticsDto>> GetStatisticsByUserAsync()
    {
        var stats = await _context.Tasks
            .Where(t => t.AssignedToId.HasValue)
            .GroupBy(t => new { t.AssignedToId, t.AssignedTo!.FirstName, t.AssignedTo.LastName })
            .Select(g => new UserStatisticsDto(
                g.Key.AssignedToId!.Value,
                g.Key.FirstName + " " + g.Key.LastName,
                g.Count(),
                g.Count(t => t.Status == TaskItemStatus.Completed),
                g.Count(t => t.Status == TaskItemStatus.InProgress),
                g.Count() == 0 ? 0 : Math.Round((double)g.Count(t => t.Status == TaskItemStatus.Completed) / g.Count() * 100, 1)))
            .ToListAsync();

        return stats;
    }

    public async Task<IReadOnlyList<TimelineStatisticsDto>> GetTimelineStatisticsAsync(string period = "daily")
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var created = await _context.Tasks
            .Where(t => t.CreatedAt >= thirtyDaysAgo)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        var completed = await _context.Tasks
            .Where(t => t.Status == TaskItemStatus.Completed && t.UpdatedAt >= thirtyDaysAgo)
            .GroupBy(t => t.UpdatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        var allDates = created.Keys.Union(completed.Keys).OrderBy(d => d);

        var timeline = allDates.Select(date => new TimelineStatisticsDto(
            date.ToString("yyyy-MM-dd"),
            created.GetValueOrDefault(date, 0),
            completed.GetValueOrDefault(date, 0)
        )).ToList();

        return timeline;
    }
}
