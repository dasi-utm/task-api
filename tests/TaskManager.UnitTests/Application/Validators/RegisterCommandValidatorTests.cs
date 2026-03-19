using FluentAssertions;
using TaskManager.Application.Commands.Auth;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;

namespace TaskManager.UnitTests.Application.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new RegisterCommand(new RegisterDto("test@example.com", "Password1", "John", "Doe"));
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldFail()
    {
        var command = new RegisterCommand(new RegisterDto("", "Password1", "John", "Doe"));
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Email"));
    }

    [Fact]
    public void Validate_WeakPassword_ShouldFail()
    {
        var command = new RegisterCommand(new RegisterDto("test@example.com", "weak", "John", "Doe"));
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyFirstName_ShouldFail()
    {
        var command = new RegisterCommand(new RegisterDto("test@example.com", "Password1", "", "Doe"));
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
