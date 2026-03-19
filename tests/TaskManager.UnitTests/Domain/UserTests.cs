using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Events;

namespace TaskManager.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_ValidInput_ReturnsUserWithCorrectProperties()
    {
        var user = User.Create("test@example.com", "hashedPassword", "John", "Doe");

        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Role.Should().Be(UserRole.User);
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ValidInput_RaisesUserCreatedEvent()
    {
        var user = User.Create("test@example.com", "hashedPassword", "John", "Doe");

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>();
    }

    [Fact]
    public void UpdateRole_ValidRole_ChangesRoleAndRaisesEvent()
    {
        var user = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        user.ClearDomainEvents();

        user.UpdateRole(UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRoleChangedEvent>();
    }
}
