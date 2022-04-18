namespace NewCraftsman.Domain.DomainProject.Validators;

using Dtos;
using FluentValidation;

public class DomainProjectDtoValidator : AbstractValidator<DomainProjectDto>
{
    public DomainProjectDtoValidator()
    {
        RuleFor(d => d.DomainName).NotEmpty();
    }
}