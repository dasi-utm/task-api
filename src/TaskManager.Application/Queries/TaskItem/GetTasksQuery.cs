using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Queries.TaskItem;

public record GetTasksQuery(
    int Page = 1,
    int PageSize = 20,
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    Guid? AssignedToId = null,
    string? SortBy = null,
    bool SortDescending = false);

public class GetTasksQueryHandler
{
    private readonly ITaskRepository _taskRepository;

    public GetTasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<PagedResult<TaskDto>> Handle(GetTasksQuery query)
    {
        var (items, total) = await _taskRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Status,
            query.Priority,
            query.AssignedToId,
            query.SortBy,
            query.SortDescending);

        var dtos = items.Select(TaskMapper.ToDto).ToList();
        return new PagedResult<TaskDto>(dtos, total, query.Page, query.PageSize);
    }
}
