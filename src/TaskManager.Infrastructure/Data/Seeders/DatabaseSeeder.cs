using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Data.Seeders;

public static class DatabaseSeeder
{
    private static readonly (string Email, string Password, string FirstName, string LastName, UserRole Role)[] SeedUsers =
    [
        ("admin@taskmanager.com", "Admin123!", "Admin", "User", UserRole.Admin),
        ("manager@taskmanager.com", "Manager123!", "Project", "Manager", UserRole.Manager),
        ("dev@taskmanager.com", "User123!", "Ion", "Popescu", UserRole.User),
    ];

    public static async Task SeedAsync(AppDbContext context)
    {
        var users = await SeedUsersAsync(context);
        if (users is not null)
            await SeedTasksAsync(context, users.Value.admin, users.Value.manager, users.Value.dev);
    }

    private static async Task<(User admin, User manager, User dev)?> SeedUsersAsync(AppDbContext context)
    {
        var seedEmails = SeedUsers.Select(u => u.Email).ToList();
        var existingEmails = await context.Users
            .Where(u => seedEmails.Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync();

        if (existingEmails.Count == SeedUsers.Length)
            return null; // all seed users already exist

        var created = new List<User>();
        foreach (var (email, password, firstName, lastName, role) in SeedUsers)
        {
            if (existingEmails.Contains(email)) continue;
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = User.Create(email, hash, firstName, lastName);
            if (role != UserRole.User) user.UpdateRole(role);
            context.Users.Add(user);
            created.Add(user);
        }

        if (created.Count == 0) return null;

        ClearDomainEvents(context);
        await context.SaveChangesAsync();

        // Reload all seed users (some may have existed before)
        var admin = await context.Users.FirstAsync(u => u.Email == "admin@taskmanager.com");
        var manager = await context.Users.FirstAsync(u => u.Email == "manager@taskmanager.com");
        var dev = await context.Users.FirstAsync(u => u.Email == "dev@taskmanager.com");

        return (admin, manager, dev);
    }

    private static async Task SeedTasksAsync(AppDbContext context, User admin, User manager, User dev)
    {
        // Skip if seed tasks already exist
        if (await context.Tasks.AnyAsync(t => t.Title == "Setup project infrastructure"))
            return;

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
