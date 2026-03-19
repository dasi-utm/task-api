using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskStatisticsService
{
    Task<TaskStatisticsDto> GetOverallStatisticsAsync();
    Task<IReadOnlyList<UserStatisticsDto>> GetStatisticsByUserAsync();
    Task<IReadOnlyList<TimelineStatisticsDto>> GetTimelineStatisticsAsync(string period = "daily");
}
