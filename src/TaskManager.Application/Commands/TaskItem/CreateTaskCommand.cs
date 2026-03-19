using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.TaskItem;

public record CreateTaskCommand(CreateTaskDto Dto, Guid UserId);

public class CreateTaskCommandHandler
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand command)
    {
        if (!Enum.TryParse<TaskPriority>(command.Dto.Priority, true, out var priority))
            return Result<TaskDto>.Failure("Invalid priority value");

        var task = Domain.Entities.TaskItem.Create(
            command.Dto.Title,
            command.Dto.Description,
            priority,
            command.Dto.DueDate,
            command.UserId);

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return Result<TaskDto>.Success(TaskMapper.ToDto(task));
    }
}
