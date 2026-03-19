using FluentAssertions;
using Moq;
using TaskManager.Application.Commands.TaskItem;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Interfaces;

namespace TaskManager.UnitTests.Application.Commands.TaskItem;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _handler = new CreateTaskCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithTaskDto()
    {
        var dto = new CreateTaskDto("Test Task", "Description", "High", DateTime.UtcNow.AddDays(7));
        var command = new CreateTaskCommand(dto, Guid.NewGuid());

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Test Task");
        result.Value.Priority.Should().Be("High");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskManager.Domain.Entities.TaskItem>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPriority_ReturnsFailure()
    {
        var dto = new CreateTaskDto("Test Task", "Description", "InvalidPriority", null);
        var command = new CreateTaskCommand(dto, Guid.NewGuid());

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid priority");
    }
}
