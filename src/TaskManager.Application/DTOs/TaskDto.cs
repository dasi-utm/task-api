namespace TaskManager.Application.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid? AssignedToId,
    Guid CreatedById,
    DateTime CreatedAt,
    DateTime UpdatedAt);
