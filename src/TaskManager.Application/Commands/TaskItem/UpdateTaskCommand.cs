using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.TaskItem;

public record UpdateTaskCommand(Guid TaskId, UpdateTaskDto Dto, Guid UserId);

public class UpdateTaskCommandHandler
{
    private readonly ITaskRepository _taskRepository;

    public UpdateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskDto>> Handle(UpdateTaskCommand command)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId);
        if (task is null)
            return Result<TaskDto>.Failure("Task not found");

        if (!Enum.TryParse<TaskPriority>(command.Dto.Priority, true, out var priority))
            return Result<TaskDto>.Failure("Invalid priority value");

        task.Update(command.Dto.Title, command.Dto.Description, priority, command.Dto.DueDate);
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        return Result<TaskDto>.Success(TaskMapper.ToDto(task));
    }
}
