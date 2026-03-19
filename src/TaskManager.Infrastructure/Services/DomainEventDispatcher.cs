using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.EventHandlers;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.Events;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<DomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            try
            {
                await DispatchSingleAsync(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch domain event {EventType}", domainEvent.GetType().Name);
            }
        }
    }

    private async Task DispatchSingleAsync(DomainEvent domainEvent)
    {
        // 1. Publish to RabbitMQ via event handlers
        switch (domainEvent)
        {
            case TaskCreatedEvent e:
                var createdHandler = _serviceProvider.GetRequiredService<TaskCreatedEventHandler>();
                await createdHandler.Handle(e);
                await NotifyTaskCreated(e);
                break;
            case TaskUpdatedEvent e:
                var updatedHandler = _serviceProvider.GetRequiredService<TaskUpdatedEventHandler>();
                await updatedHandler.Handle(e);
                await NotifyTaskUpdated(e);
                break;
            case TaskStatusChangedEvent e:
                var statusHandler = _serviceProvider.GetRequiredService<TaskStatusChangedEventHandler>();
                await statusHandler.Handle(e);
                await NotifyStatusChanged(e);
                break;
            case TaskDeletedEvent e:
                var deletedHandler = _serviceProvider.GetRequiredService<TaskDeletedEventHandler>();
                await deletedHandler.Handle(e);
                await NotifyTaskDeleted(e);
                break;
            default:
                _logger.LogWarning("No handler registered for event type {EventType}", domainEvent.GetType().Name);
                break;
        }
    }

    // 2. Push to SignalR for real-time frontend updates
    private async Task NotifyTaskCreated(TaskCreatedEvent e)
    {
        try
        {
            var notifier = _serviceProvider.GetService<ITaskNotificationService>();
            if (notifier is null) return;
            var repo = _serviceProvider.GetRequiredService<ITaskRepository>();
            var task = await repo.GetByIdAsync(e.TaskId);
            if (task is not null)
                await notifier.TaskCreated(TaskMapper.ToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for TaskCreated");
        }
    }

    private async Task NotifyTaskUpdated(TaskUpdatedEvent e)
    {
        try
        {
            var notifier = _serviceProvider.GetService<ITaskNotificationService>();
            if (notifier is null) return;
            var repo = _serviceProvider.GetRequiredService<ITaskRepository>();
            var task = await repo.GetByIdAsync(e.TaskId);
            if (task is not null)
                await notifier.TaskUpdated(TaskMapper.ToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for TaskUpdated");
        }
    }

    private async Task NotifyStatusChanged(TaskStatusChangedEvent e)
    {
        try
        {
            var notifier = _serviceProvider.GetService<ITaskNotificationService>();
            if (notifier is not null)
                await notifier.TaskStatusChanged(e.TaskId, e.NewStatus.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for TaskStatusChanged");
        }
    }

    private async Task NotifyTaskDeleted(TaskDeletedEvent e)
    {
        try
        {
            var notifier = _serviceProvider.GetService<ITaskNotificationService>();
            if (notifier is not null)
                await notifier.TaskDeleted(e.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for TaskDeleted");
        }
    }
}
