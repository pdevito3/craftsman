namespace Craftsman.Tests.ModelTests;

using Bogus;
using FluentAssertions;
using Models;
using Xunit;

public class ApiTemplateTests
{
    [Fact]
    public void policyname_gets_snake_cased()
    {
        var project = new ApiTemplate()
        {
            ProjectName = "ProjectName"
        };
        project.PolicyName.Should().Be("project_name");
    }
    [Fact]
    public void policyname_is_set_value_if_given()
    {
        var given = new Faker().Lorem.Word();
        var project = new ApiTemplate()
        {
            ProjectName = given
        };
        project.PolicyName.Should().Be(given);
    }
    
    
    [Fact]
    public void dockerconfig_defaults_are_set()
    {
        var given = new Faker().Lorem.Word();
        var project = new ApiTemplate()
        {
            ProjectName = given
        };
        project.DockerConfig.ProjectName.Should().Be(given);
    }
}