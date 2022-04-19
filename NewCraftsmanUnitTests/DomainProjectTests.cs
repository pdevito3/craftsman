namespace NewCraftsmanUnitTests;

using Fakes;
using FluentAssertions;
using NewCraftsman.Domain.DomainProjects;
using NUnit.Framework;

public class DomainProjectTests
{
    [Test]
    public void addgit_defaults_to_true()
    {
        var dto = new FakeDomainProjectDto().Generate();
        dto.AddGit = null;
        
        var project = DomainProject.Create(dto);

        project.AddGit.Should().BeTrue();
    }
    
    [Test]
    public void UseSystemGitUser_defaults_to_true()
    {
        var dto = new FakeDomainProjectDto().Generate();
        dto.UseSystemGitUser = null;
        
        var project = DomainProject.Create(dto);

        project.UseSystemGitUser.Should().BeTrue();
    }
    
    [Test]
    public void domain_name_required()
    {
        var dto = new FakeDomainProjectDto().Generate();
        dto.DomainName = null;

        FluentActions
            .Invoking( () =>
            {
                DomainProject.Create(dto);
            })
            .Should()
            .Throw<FluentValidation.ValidationException>();
    }
}