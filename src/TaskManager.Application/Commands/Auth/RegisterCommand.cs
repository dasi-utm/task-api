using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.Auth;

public record RegisterCommand(RegisterDto Dto);

public class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(RegisterCommand command)
    {
        if (await _userRepository.EmailExistsAsync(command.Dto.Email))
            return Result<UserDto>.Failure("Email is already registered");

        var passwordHash = _passwordHasher.HashPassword(command.Dto.Password);
        var user = User.Create(command.Dto.Email, passwordHash, command.Dto.FirstName, command.Dto.LastName);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return Result<UserDto>.Success(new UserDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString()));
    }
}
