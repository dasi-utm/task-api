using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Events;

namespace TaskManager.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.User;

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.User,
            CreatedBy = email
        };

        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, user.Email));
        return user;
    }

    public void UpdateRole(UserRole newRole)
    {
        var oldRole = Role;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserRoleChangedEvent(Id, oldRole, newRole));
    }
}
