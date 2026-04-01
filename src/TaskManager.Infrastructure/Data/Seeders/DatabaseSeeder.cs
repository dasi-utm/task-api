using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Data.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        var admin = User.Create("admin@taskmanager.com", adminHash, "Admin", "User");
        admin.UpdateRole(UserRole.Admin);

        var managerHash = BCrypt.Net.BCrypt.HashPassword("Manager123!");
        var manager = User.Create("manager@taskmanager.com", managerHash, "Project", "Manager");
        manager.UpdateRole(UserRole.Manager);

        var userHash = BCrypt.Net.BCrypt.HashPassword("User123!");
        var dev = User.Create("dev@taskmanager.com", userHash, "Ion", "Popescu");

        context.Users.AddRange(admin, manager, dev);
        ClearDomainEvents(context);
        await context.SaveChangesAsync();

        var tasks = new[]
        {
            TaskItem.Create("Setup project infrastructure", "Configure Docker, CI/CD pipelines, and development environment", TaskPriority.Critical, DateTime.UtcNow.AddDays(3), admin.Id),
            TaskItem.Create("Design database schema", "Create ERD and define entity relationships for the task management system", TaskPriority.High, DateTime.UtcNow.AddDays(5), admin.Id),
            TaskItem.Create("Implement user authentication", "JWT-based auth with register, login, and role-based access", TaskPriority.High, DateTime.UtcNow.AddDays(7), dev.Id),
            TaskItem.Create("Build task CRUD endpoints", "REST API for creating, reading, updating, and deleting tasks", TaskPriority.High, DateTime.UtcNow.AddDays(10), dev.Id),
            TaskItem.Create("Create React dashboard", "Frontend dashboard with task board, filters, and real-time updates", TaskPriority.Medium, DateTime.UtcNow.AddDays(14), dev.Id),
            TaskItem.Create("Setup RabbitMQ integration", "Configure message broker for domain event publishing between services", TaskPriority.Medium, DateTime.UtcNow.AddDays(7), admin.Id),
            TaskItem.Create("Write unit tests", "Cover domain logic and application handlers with xUnit tests", TaskPriority.Medium, DateTime.UtcNow.AddDays(12), dev.Id),
            TaskItem.Create("Configure monitoring", "Setup logging, health checks, and observability tooling", TaskPriority.Low, DateTime.UtcNow.AddDays(20), manager.Id),
            TaskItem.Create("Write API documentation", "Document all endpoints with Swagger annotations and examples", TaskPriority.Low, DateTime.UtcNow.AddDays(15), manager.Id),
            TaskItem.Create("Performance testing", "Load test critical endpoints and optimize bottlenecks", TaskPriority.Low, null, manager.Id),
        };

        context.Tasks.AddRange(tasks);
        ClearDomainEvents(context);
        await context.SaveChangesAsync();

        // Move some tasks to different statuses for realistic data
        tasks[0].ChangeStatus(TaskItemStatus.InProgress);
        tasks[1].ChangeStatus(TaskItemStatus.InProgress);
        tasks[1].ChangeStatus(TaskItemStatus.Completed);
        tasks[2].ChangeStatus(TaskItemStatus.InProgress);
        tasks[5].ChangeStatus(TaskItemStatus.InProgress);
        tasks[5].ChangeStatus(TaskItemStatus.Completed);
        tasks[8].ChangeStatus(TaskItemStatus.Cancelled);

        // Assign some tasks
        tasks[0].AssignTo(dev.Id);
        tasks[2].AssignTo(dev.Id);
        tasks[3].AssignTo(dev.Id);
        tasks[7].AssignTo(manager.Id);

        ClearDomainEvents(context);
        await context.SaveChangesAsync();
    }

    private static void ClearDomainEvents(AppDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            entry.Entity.ClearDomainEvents();
    }
}
