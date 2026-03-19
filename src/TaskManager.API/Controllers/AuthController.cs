using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Commands.Auth;
using TaskManager.Application.DTOs;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _registerHandler;
    private readonly LoginCommandHandler _loginHandler;
    private readonly IValidator<RegisterCommand> _registerValidator;
    private readonly IValidator<LoginCommand> _loginValidator;

    public AuthController(
        RegisterCommandHandler registerHandler,
        LoginCommandHandler loginHandler,
        IValidator<RegisterCommand> registerValidator,
        IValidator<LoginCommand> loginValidator)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var command = new RegisterCommand(dto);
        var validation = await _registerValidator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            var details = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new ErrorResponse("Validation failed", details));
        }

        var result = await _registerHandler.Handle(command);
        return result.IsSuccess
            ? Created($"api/v1/users/{result.Value!.Id}", result.Value)
            : BadRequest(new ErrorResponse(result.Error!));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var command = new LoginCommand(dto);
        var validation = await _loginValidator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            var details = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new ErrorResponse("Validation failed", details));
        }

        var result = await _loginHandler.Handle(command);
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new ErrorResponse(result.Error!));
    }
}
