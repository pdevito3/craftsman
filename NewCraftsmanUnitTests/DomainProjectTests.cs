namespace NewCraftsmanUnitTests;

using Craftsman.Domain;
using Fakes;
using FluentAssertions;
using NUnit.Framework;

public class DomainProjectTests
{
    [Fact]
    public void addgit_defaults_to_true()
    {
        var dto = new FakeDomainProjectDto().Generate();
        dto.AddGit = null;
        
        var project = new DomainProject(dto);

        project.AddGit.Should().BeTrue();
    }
    
    [Fact]
    public void UseSystemGitUser_defaults_to_true()
    {
        var dto = new FakeDomainProjectDto().Generate();
        dto.UseSystemGitUser = null;
        
        var project = new DomainProject(dto);

        project.UseSystemGitUser.Should().BeTrue();
    }
    
    [Fact]
    public void domain_name_required()
    {
        var dto = new FakeDomainProjectDto().Generate();
        dto.DomainName = null;

        FluentActions
            .Invoking( () =>
            {
                var _ = new DomainProject(dto);
            })
            .Should()
            .Throw<FluentValidation.ValidationException>();
    }
}