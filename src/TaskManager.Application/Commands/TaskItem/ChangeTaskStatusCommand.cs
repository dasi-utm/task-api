using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.TaskItem;

public record ChangeTaskStatusCommand(Guid TaskId, string Status);

public class ChangeTaskStatusCommandHandler
{
    private readonly ITaskRepository _taskRepository;

    public ChangeTaskStatusCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskDto>> Handle(ChangeTaskStatusCommand command)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId);
        if (task is null)
            return Result<TaskDto>.Failure("Task not found");

        if (!Enum.TryParse<TaskItemStatus>(command.Status, true, out var newStatus))
            return Result<TaskDto>.Failure("Invalid status value");

        try
        {
            task.ChangeStatus(newStatus);
        }
        catch (InvalidOperationException ex)
        {
            return Result<TaskDto>.Failure(ex.Message);
        }

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        return Result<TaskDto>.Success(TaskMapper.ToDto(task));
    }
}
