namespace NewCraftsmanUnitTests;

using Craftsman.Domain;
using Fakes;
using FluentAssertions;
using NUnit.Framework;

public class DbContextConfigTests
{
    [Test]
    public void provider_defaults_to_postgres()
    {
        var dto = new FakeDbContextConfigDto().Generate();
        dto.Provider = null;
        
        var project = DbContextConfig.Create(dto);

        project.ProviderEnum.Should().Be(DbProvider.Postgres);
    }
    
    [Test]
    public void naming_convention_defaults_to_snakecase()
    {
        var dto = new FakeDbContextConfigDto().Generate();
        dto.NamingConvention = null;
        
        var project = DbContextConfig.Create(dto);

        project.NamingConventionEnum.Should().Be(NamingConventionEnum.SnakeCase);
    }
    
    [Test]
    public void database_name_required()
    {
        var dto = new FakeDbContextConfigDto().Generate();
        dto.DatabaseName = null;

        FluentActions
            .Invoking( () =>
            {
                DbContextConfig.Create(dto);
            })
            .Should()
            .Throw<FluentValidation.ValidationException>();
    }
    
    [Test]
    public void context_name_required()
    {
        var dto = new FakeDbContextConfigDto().Generate();
        dto.ContextName = null;

        FluentActions
            .Invoking( () =>
            {
                DbContextConfig.Create(dto);
            })
            .Should()
            .Throw<FluentValidation.ValidationException>();
    }
}