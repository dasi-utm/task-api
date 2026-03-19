using FluentAssertions;
using TaskManager.Application.Commands.TaskItem;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;

namespace TaskManager.UnitTests.Application.Validators;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateTaskCommand(
            new CreateTaskDto("Valid Title", "Description", "High", DateTime.UtcNow.AddDays(7)),
            Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldFail()
    {
        var command = new CreateTaskCommand(
            new CreateTaskDto("", "Description", "High", null),
            Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Title"));
    }

    [Fact]
    public void Validate_TitleTooLong_ShouldFail()
    {
        var command = new CreateTaskCommand(
            new CreateTaskDto(new string('A', 201), "Description", "High", null),
            Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidPriority_ShouldFail()
    {
        var command = new CreateTaskCommand(
            new CreateTaskDto("Title", "Description", "NotAPriority", null),
            Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_PastDueDate_ShouldFail()
    {
        var command = new CreateTaskCommand(
            new CreateTaskDto("Title", "Description", "High", DateTime.UtcNow.AddDays(-1)),
            Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
