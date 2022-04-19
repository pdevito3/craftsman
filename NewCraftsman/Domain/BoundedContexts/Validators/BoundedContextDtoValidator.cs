namespace NewCraftsman.Domain.BoundedContexts.Validators;

using Dtos;
using FluentValidation;

public class BoundedContextDtoValidator : AbstractValidator<BoundedContextDto>
{
    public BoundedContextDtoValidator()
    {
        RuleFor(d => d.ProjectName).NotEmpty();
    }
}