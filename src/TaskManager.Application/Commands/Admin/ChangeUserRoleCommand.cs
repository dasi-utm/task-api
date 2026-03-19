using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Commands.Admin;

public record ChangeUserRoleCommand(Guid UserId, string Role);

public class ChangeUserRoleCommandHandler
{
    private readonly IUserRepository _userRepository;

    public ChangeUserRoleCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(ChangeUserRoleCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            return Result<UserDto>.Failure("User not found");

        if (!Enum.TryParse<UserRole>(command.Role, true, out var newRole))
            return Result<UserDto>.Failure("Invalid role value. Allowed: User, Manager, Admin");

        user.UpdateRole(newRole);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return Result<UserDto>.Success(
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString()));
    }
}
