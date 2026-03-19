namespace TaskManager.Application.DTOs;

public record UserStatisticsDto(
    Guid UserId,
    string Name,
    int TotalAssigned,
    int Completed,
    int InProgress,
    double CompletionRate);
