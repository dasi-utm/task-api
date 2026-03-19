using Microsoft.Extensions.Logging;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Events;

namespace TaskManager.Application.EventHandlers;

public class TaskCreatedEventHandler
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TaskCreatedEventHandler> _logger;

    public TaskCreatedEventHandler(IEventPublisher eventPublisher, ILogger<TaskCreatedEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(TaskCreatedEvent domainEvent)
    {
        _logger.LogInformation("Handling TaskCreatedEvent for Task {TaskId}", domainEvent.TaskId);

        var message = new
        {
            eventType = "TaskCreated",
            timestamp = domainEvent.OccurredOn.ToString("O"),
            correlationId = domainEvent.EventId,
            payload = new
            {
                taskId = domainEvent.TaskId,
                title = domainEvent.Title,
                createdBy = domainEvent.CreatedById
            }
        };

        try
        {
            await _eventPublisher.PublishAsync("task.created", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish TaskCreatedEvent for Task {TaskId}", domainEvent.TaskId);
        }
    }
}
