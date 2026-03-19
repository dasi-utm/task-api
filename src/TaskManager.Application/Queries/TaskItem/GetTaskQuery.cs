using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Queries.TaskItem;

public record GetTaskQuery(Guid Id);

public class GetTaskQueryHandler
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskQuery query)
    {
        var task = await _taskRepository.GetByIdAsync(query.Id);
        if (task is null)
            return Result<TaskDto>.Failure("Task not found");

        return Result<TaskDto>.Success(TaskMapper.ToDto(task));
    }
}
