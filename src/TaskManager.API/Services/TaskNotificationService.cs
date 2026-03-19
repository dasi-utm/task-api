using Microsoft.AspNetCore.SignalR;
using TaskManager.API.Hubs;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Services;

public class TaskNotificationService : ITaskNotificationService
{
    private readonly IHubContext<TaskHub> _hubContext;

    public TaskNotificationService(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task TaskCreated(TaskDto task)
    {
        await _hubContext.Clients.All.SendAsync("TaskCreated", task);
    }

    public async Task TaskUpdated(TaskDto task)
    {
        await _hubContext.Clients.All.SendAsync("TaskUpdated", task);
    }

    public async Task TaskDeleted(Guid taskId)
    {
        await _hubContext.Clients.All.SendAsync("TaskDeleted", taskId);
    }

    public async Task TaskStatusChanged(Guid taskId, string newStatus)
    {
        await _hubContext.Clients.All.SendAsync("TaskStatusChanged", taskId, newStatus);
    }
}
