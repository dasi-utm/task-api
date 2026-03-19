using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Commands.TaskItem;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Queries.TaskItem;
using TaskManager.Domain.Enums;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly CreateTaskCommandHandler _createHandler;
    private readonly UpdateTaskCommandHandler _updateHandler;
    private readonly DeleteTaskCommandHandler _deleteHandler;
    private readonly ChangeTaskStatusCommandHandler _changeStatusHandler;
    private readonly AssignTaskCommandHandler _assignHandler;
    private readonly GetTaskQueryHandler _getTaskHandler;
    private readonly GetTasksQueryHandler _getTasksHandler;
    private readonly IValidator<CreateTaskCommand> _createValidator;
    private readonly IValidator<UpdateTaskCommand> _updateValidator;

    public TasksController(
        CreateTaskCommandHandler createHandler,
        UpdateTaskCommandHandler updateHandler,
        DeleteTaskCommandHandler deleteHandler,
        ChangeTaskStatusCommandHandler changeStatusHandler,
        AssignTaskCommandHandler assignHandler,
        GetTaskQueryHandler getTaskHandler,
        GetTasksQueryHandler getTasksHandler,
        IValidator<CreateTaskCommand> createValidator,
        IValidator<UpdateTaskCommand> updateValidator)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _changeStatusHandler = changeStatusHandler;
        _assignHandler = assignHandler;
        _getTaskHandler = getTaskHandler;
        _getTasksHandler = getTasksHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    private Guid CurrentUserId => Guid.Parse(
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException());

    /// <summary>
    /// Lists tasks with pagination and filtering.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TaskItemStatus? status = null,
        [FromQuery] TaskPriority? priority = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetTasksQuery(page, pageSize, status, priority, assignedToId, sortBy, sortDescending);
        var result = await _getTasksHandler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single task by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getTaskHandler.Handle(new GetTaskQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new ErrorResponse(result.Error!));
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var command = new CreateTaskCommand(dto, CurrentUserId);
        var validation = await _createValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new ErrorResponse("Validation failed",
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await _createHandler.Handle(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new ErrorResponse(result.Error!));
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto)
    {
        var command = new UpdateTaskCommand(id, dto, CurrentUserId);
        var validation = await _updateValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new ErrorResponse("Validation failed",
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await _updateHandler.Handle(command);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new ErrorResponse(result.Error!));
    }

    /// <summary>
    /// Soft-deletes a task.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _deleteHandler.Handle(new DeleteTaskCommand(id, CurrentUserId));
        return result.IsSuccess ? NoContent() : NotFound(new ErrorResponse(result.Error!));
    }

    /// <summary>
    /// Changes the status of a task.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        var result = await _changeStatusHandler.Handle(new ChangeTaskStatusCommand(id, dto.Status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new ErrorResponse(result.Error!));
    }

    /// <summary>
    /// Assigns a task to a user.
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignTaskDto dto)
    {
        var result = await _assignHandler.Handle(new AssignTaskCommand(id, dto.UserId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new ErrorResponse(result.Error!));
    }
}
