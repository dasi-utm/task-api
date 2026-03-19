using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Events;

namespace TaskManager.UnitTests.Domain;

public class TaskItemTests
{
    [Fact]
    public void Create_ValidInput_ReturnsTaskWithCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var task = TaskItem.Create("Test", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), userId);

        task.Title.Should().Be("Test");
        task.Description.Should().Be("Description");
        task.Priority.Should().Be(TaskPriority.High);
        task.Status.Should().Be(TaskItemStatus.Pending);
        task.CreatedById.Should().Be(userId);
    }

    [Fact]
    public void Create_ValidInput_RaisesTaskCreatedEvent()
    {
        var task = TaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());

        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskCreatedEvent>();
    }

    [Fact]
    public void Update_ValidInput_UpdatesPropertiesAndRaisesEvent()
    {
        var task = TaskItem.Create("Old", "Old Desc", TaskPriority.Low, null, Guid.NewGuid());
        task.ClearDomainEvents();

        task.Update("New", "New Desc", TaskPriority.Critical, DateTime.UtcNow.AddDays(5));

        task.Title.Should().Be("New");
        task.Priority.Should().Be(TaskPriority.Critical);
        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskUpdatedEvent>();
    }

    [Fact]
    public void ChangeStatus_ValidTransition_ChangesStatusAndRaisesEvent()
    {
        var task = TaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());
        task.ClearDomainEvents();

        task.ChangeStatus(TaskItemStatus.InProgress);

        task.Status.Should().Be(TaskItemStatus.InProgress);
        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskStatusChangedEvent>();
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ThrowsException()
    {
        var task = TaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());

        var act = () => task.ChangeStatus(TaskItemStatus.Completed);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangeStatus_CompletedFromInProgress_Succeeds()
    {
        var task = TaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());
        task.ChangeStatus(TaskItemStatus.InProgress);
        task.ClearDomainEvents();

        task.ChangeStatus(TaskItemStatus.Completed);

        task.Status.Should().Be(TaskItemStatus.Completed);
    }

    [Fact]
    public void AssignTo_ValidUser_SetsAssignedToAndRaisesEvent()
    {
        var task = TaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());
        task.ClearDomainEvents();
        var userId = Guid.NewGuid();

        task.AssignTo(userId);

        task.AssignedToId.Should().Be(userId);
        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskAssignedEvent>();
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedAndRaisesEvent()
    {
        var task = TaskItem.Create("Test", "Desc", TaskPriority.Medium, null, Guid.NewGuid());
        task.ClearDomainEvents();

        task.SoftDelete();

        task.IsDeleted.Should().BeTrue();
        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskDeletedEvent>();
    }
}
