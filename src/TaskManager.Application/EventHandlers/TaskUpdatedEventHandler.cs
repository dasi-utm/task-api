using Microsoft.Extensions.Logging;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Events;

namespace TaskManager.Application.EventHandlers;

public class TaskUpdatedEventHandler
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TaskUpdatedEventHandler> _logger;

    public TaskUpdatedEventHandler(IEventPublisher eventPublisher, ILogger<TaskUpdatedEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(TaskUpdatedEvent domainEvent)
    {
        var message = new
        {
            eventType = "TaskUpdated",
            timestamp = domainEvent.OccurredOn.ToString("O"),
            correlationId = domainEvent.EventId,
            payload = new { taskId = domainEvent.TaskId, title = domainEvent.Title }
        };

        try
        {
            await _eventPublisher.PublishAsync("task.updated", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish TaskUpdatedEvent for Task {TaskId}", domainEvent.TaskId);
        }
    }
}
