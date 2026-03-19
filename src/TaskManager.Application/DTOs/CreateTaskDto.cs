namespace TaskManager.Application.DTOs;

public record CreateTaskDto(string Title, string Description, string Priority, DateTime? DueDate);
