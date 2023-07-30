namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain.Enums;
using Helpers;
using Services;

public class CreateUserRoleUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CreateUserRoleUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName)
    {
        var entityName = "User";
        var entityPlural = "Users";
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"CreateUserRoleTests.cs", entityPlural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, srcDirectory, classPath, entityName, entityPlural, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string entityName, string entityPlural, string projectBaseName)
    {
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var roleClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entityName, projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", entityPlural, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {roleClassPath.ClassNamespace};
using {domainEventsClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using Xunit;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        _faker = new Faker();
    }}
    
    [Fact]
    public void can_create_valid_userRole()
    {{
        // Arrange
        var user = new FakeUserBuilder().Build();
        var role = _faker.PickRandom(Role.ListNames());
        
        // Act
        var fakeUserRole = UserRole.Create(user, new Role(role));

        // Assert
        fakeUserRole.User.Id.Should().Be(user.Id);
        fakeUserRole.Role.Should().Be(new Role(role));
    }}

    [Fact]
    public void queue_domain_event_on_create()
    {{
        // Arrange
        var user = new FakeUserBuilder().Build();
        var role = _faker.PickRandom(Role.ListNames());
        
        // Act
        var fakeUserRole = UserRole.Create(user, new Role(role));

        // Assert
        fakeUserRole.DomainEvents.Count.Should().Be(1);
        fakeUserRole.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(UserRolesUpdated));
    }}
}}";
    }
}
