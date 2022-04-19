namespace NewCraftsman.Domain.DbContextConfigs.Validators;

using Dtos;
using FluentValidation;

public class DbContextConfigDtoValidator : AbstractValidator<DbContextConfigDto>
{
    public DbContextConfigDtoValidator()
    {
        RuleFor(d => d.ContextName).NotEmpty();
        RuleFor(d => d.DatabaseName).NotEmpty();
    }
}