namespace TaskManager.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
}
