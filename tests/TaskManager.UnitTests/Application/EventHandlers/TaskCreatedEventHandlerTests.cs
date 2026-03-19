using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Application.EventHandlers;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Events;

namespace TaskManager.UnitTests.Application.EventHandlers;

public class TaskCreatedEventHandlerTests
{
    private readonly Mock<IEventPublisher> _publisherMock;
    private readonly TaskCreatedEventHandler _handler;

    public TaskCreatedEventHandlerTests()
    {
        _publisherMock = new Mock<IEventPublisher>();
        var loggerMock = new Mock<ILogger<TaskCreatedEventHandler>>();
        _handler = new TaskCreatedEventHandler(_publisherMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidEvent_PublishesToRabbitMQ()
    {
        var domainEvent = new TaskCreatedEvent(Guid.NewGuid(), "Test Task", Guid.NewGuid());

        await _handler.Handle(domainEvent);

        _publisherMock.Verify(p => p.PublishAsync("task.created", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PublisherThrows_DoesNotRethrow()
    {
        var domainEvent = new TaskCreatedEvent(Guid.NewGuid(), "Test Task", Guid.NewGuid());
        _publisherMock.Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("RabbitMQ down"));

        var act = () => _handler.Handle(domainEvent);

        await act.Should().NotThrowAsync();
    }
}
