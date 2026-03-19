using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _eventDispatcher;

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public AppDbContext(DbContextOptions<AppDbContext> options, IDomainEventDispatcher? eventDispatcher = null)
        : base(options)
    {
        _eventDispatcher = eventDispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        var domainEvents = CollectDomainEvents();
        var result = await base.SaveChangesAsync(cancellationToken);

        if (_eventDispatcher is not null && domainEvents.Count > 0)
        {
            await _eventDispatcher.DispatchAsync(domainEvents);
        }

        return result;
    }

    private void UpdateAuditFields()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private List<DomainEvent> CollectDomainEvents()
    {
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entity in entities)
        {
            entity.Entity.ClearDomainEvents();
        }

        return domainEvents;
    }
}
