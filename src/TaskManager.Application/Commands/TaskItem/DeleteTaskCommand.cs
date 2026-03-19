using TaskManager.Application.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.TaskItem;

public record DeleteTaskCommand(Guid TaskId, Guid UserId);

public class DeleteTaskCommandHandler
{
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<bool>> Handle(DeleteTaskCommand command)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId);
        if (task is null)
            return Result<bool>.Failure("Task not found");

        task.SoftDelete();
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
