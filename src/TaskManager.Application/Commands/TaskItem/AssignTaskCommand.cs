using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.TaskItem;

public record AssignTaskCommand(Guid TaskId, Guid UserId);

public class AssignTaskCommandHandler
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public AssignTaskCommandHandler(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<TaskDto>> Handle(AssignTaskCommand command)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId);
        if (task is null)
            return Result<TaskDto>.Failure("Task not found");

        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            return Result<TaskDto>.Failure("User not found");

        task.AssignTo(command.UserId);
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        return Result<TaskDto>.Success(TaskMapper.ToDto(task));
    }
}
