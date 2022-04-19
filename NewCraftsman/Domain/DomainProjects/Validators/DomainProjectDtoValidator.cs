namespace NewCraftsman.Domain.DomainProjects.Validators;

using Dtos;
using FluentValidation;

public class DomainProjectDtoValidator : AbstractValidator<DomainProjectDto>
{
    public DomainProjectDtoValidator()
    {
        RuleFor(d => d.DomainName).NotEmpty();
        RuleFor(d => d.BoundedContexts).NotEmpty();
    }
}