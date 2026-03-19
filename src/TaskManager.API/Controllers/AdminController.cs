using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Commands.Admin;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Queries.User;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly GetUsersQueryHandler _getUsersHandler;
    private readonly ChangeUserRoleCommandHandler _changeRoleHandler;

    public AdminController(
        GetUsersQueryHandler getUsersHandler,
        ChangeUserRoleCommandHandler changeRoleHandler)
    {
        _getUsersHandler = getUsersHandler;
        _changeRoleHandler = changeRoleHandler;
    }

    /// <summary>
    /// Lists all users (admin only).
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _getUsersHandler.Handle(new GetUsersQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Changes a user's role (admin only).
    /// </summary>
    [HttpPatch("users/{id:guid}/role")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleDto dto)
    {
        var result = await _changeRoleHandler.Handle(new ChangeUserRoleCommand(id, dto.Role));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new ErrorResponse(result.Error!));
    }
}

public record ChangeRoleDto(string Role);
