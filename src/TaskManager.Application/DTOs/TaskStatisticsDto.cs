namespace TaskManager.Application.DTOs;

public record TaskStatisticsDto(
    int TotalTasks,
    Dictionary<string, int> ByStatus,
    Dictionary<string, int> ByPriority,
    int OverdueCount);
