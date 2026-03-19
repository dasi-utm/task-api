using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.Auth;

public record LoginCommand(LoginDto Dto);

public class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Dto.Email);
        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid email or password");

        if (!_passwordHasher.VerifyPassword(command.Dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Invalid email or password");

        var token = _tokenService.GenerateToken(user);
        var userDto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString());

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            token,
            DateTime.UtcNow.AddHours(1),
            userDto));
    }
}
