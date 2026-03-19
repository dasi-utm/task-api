using Microsoft.Extensions.Logging;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Events;

namespace TaskManager.Application.EventHandlers;

public class TaskDeletedEventHandler
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TaskDeletedEventHandler> _logger;

    public TaskDeletedEventHandler(IEventPublisher eventPublisher, ILogger<TaskDeletedEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(TaskDeletedEvent domainEvent)
    {
        var message = new
        {
            eventType = "TaskDeleted",
            timestamp = domainEvent.OccurredOn.ToString("O"),
            correlationId = domainEvent.EventId,
            payload = new { taskId = domainEvent.TaskId }
        };

        try
        {
            await _eventPublisher.PublishAsync("task.deleted", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish TaskDeletedEvent for Task {TaskId}", domainEvent.TaskId);
        }
    }
}
