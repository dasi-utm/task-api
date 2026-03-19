namespace TaskManager.Application.DTOs;

public record UpdateTaskDto(string Title, string Description, string Priority, DateTime? DueDate);
