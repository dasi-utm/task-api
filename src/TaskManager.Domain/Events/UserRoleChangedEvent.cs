using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Events;

public class UserRoleChangedEvent : DomainEvent
{
    public Guid UserId { get; }
    public UserRole OldRole { get; }
    public UserRole NewRole { get; }

    public UserRoleChangedEvent(Guid userId, UserRole oldRole, UserRole newRole)
    {
        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
    }
}
