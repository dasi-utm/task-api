using FluentAssertions;
using Moq;
using TaskManager.Application.Commands.TaskItem;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using DomainTaskItem = TaskManager.Domain.Entities.TaskItem;

namespace TaskManager.UnitTests.Application.Commands.TaskItem;

public class ChangeTaskStatusCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly ChangeTaskStatusCommandHandler _handler;

    public ChangeTaskStatusCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _handler = new ChangeTaskStatusCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTransition_ReturnsSuccess()
    {
        var task = DomainTaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());
        _repositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        var result = await _handler.Handle(new ChangeTaskStatusCommand(task.Id, "InProgress"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task Handle_InvalidTransition_ReturnsFailure()
    {
        var task = DomainTaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());
        _repositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        var result = await _handler.Handle(new ChangeTaskStatusCommand(task.Id, "Completed"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot transition");
    }

    [Fact]
    public async Task Handle_TaskNotFound_ReturnsFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DomainTaskItem?)null);

        var result = await _handler.Handle(new ChangeTaskStatusCommand(Guid.NewGuid(), "InProgress"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}
