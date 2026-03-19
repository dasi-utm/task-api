using FluentValidation;
using TaskManager.Application.Commands.TaskItem;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Dto.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Dto.Priority)
            .Must(p => Enum.TryParse<TaskPriority>(p, true, out _))
            .WithMessage("Invalid priority value. Allowed: Low, Medium, High, Critical");

        RuleFor(x => x.Dto.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.Dto.DueDate.HasValue);
    }
}
