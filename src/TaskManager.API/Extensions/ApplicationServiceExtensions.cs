using FluentValidation;
using TaskManager.Application.Commands.Admin;
using TaskManager.Application.Commands.Auth;
using TaskManager.Application.Commands.TaskItem;
using TaskManager.Application.EventHandlers;
using TaskManager.Application.Queries.TaskItem;
using TaskManager.Application.Queries.User;

namespace TaskManager.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(TaskManager.Application.Common.Result<>).Assembly);

        // Auth Handlers
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();

        // Task Command Handlers
        services.AddScoped<CreateTaskCommandHandler>();
        services.AddScoped<UpdateTaskCommandHandler>();
        services.AddScoped<DeleteTaskCommandHandler>();
        services.AddScoped<ChangeTaskStatusCommandHandler>();
        services.AddScoped<AssignTaskCommandHandler>();

        // Task Query Handlers
        services.AddScoped<GetTaskQueryHandler>();
        services.AddScoped<GetTasksQueryHandler>();

        // Admin Handlers
        services.AddScoped<ChangeUserRoleCommandHandler>();
        services.AddScoped<GetUsersQueryHandler>();

        // Event Handlers
        services.AddScoped<TaskCreatedEventHandler>();
        services.AddScoped<TaskUpdatedEventHandler>();
        services.AddScoped<TaskStatusChangedEventHandler>();
        services.AddScoped<TaskDeletedEventHandler>();

        return services;
    }
}
