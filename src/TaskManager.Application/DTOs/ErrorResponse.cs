namespace TaskManager.Application.DTOs;

public record ErrorResponse(string Error, object? Details = null);
