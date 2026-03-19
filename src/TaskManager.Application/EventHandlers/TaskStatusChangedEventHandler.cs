using Microsoft.Extensions.Logging;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Events;

namespace TaskManager.Application.EventHandlers;

public class TaskStatusChangedEventHandler
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TaskStatusChangedEventHandler> _logger;

    public TaskStatusChangedEventHandler(IEventPublisher eventPublisher, ILogger<TaskStatusChangedEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(TaskStatusChangedEvent domainEvent)
    {
        var message = new
        {
            eventType = "TaskStatusChanged",
            timestamp = domainEvent.OccurredOn.ToString("O"),
            correlationId = domainEvent.EventId,
            payload = new
            {
                taskId = domainEvent.TaskId,
                oldStatus = domainEvent.OldStatus.ToString(),
                newStatus = domainEvent.NewStatus.ToString()
            }
        };

        try
        {
            await _eventPublisher.PublishAsync("task.status-changed", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish TaskStatusChangedEvent for Task {TaskId}", domainEvent.TaskId);
        }
    }
}
