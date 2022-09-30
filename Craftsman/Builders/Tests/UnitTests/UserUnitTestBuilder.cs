namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Helpers;
using Services;

public class UserUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public UserUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"CreateUserTests.cs", "Users", projectBaseName);
        var fileText = CreateFileText(solutionDirectory, srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public void UpdateTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"UpdateUserTests.cs", "Users", projectBaseName);
        var fileText = UpdateFileText(solutionDirectory, srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string CreateFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var emailClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Emails", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Users", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "Users", projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", "Users", projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", "User", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {domainEventsClassPath.ClassNamespace};
using {emailClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using NUnit.Framework;

[Parallelizable]
public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public CreateUserTests()
    {{
        _faker = new Faker();
    }}
    
    [Test]
    public void can_create_valid_user()
    {{
        // Arrange
        var toCreate = new FakeUserForCreationDto().Generate();

        // Act
        var newUser = User.Create(toCreate);
        
        // Assert
        newUser.Identifier.Should().Be(toCreate.Identifier);
        newUser.FirstName.Should().Be(toCreate.FirstName);
        newUser.LastName.Should().Be(toCreate.LastName);
        newUser.Email.Should().Be(new Email(toCreate.Email));
        newUser.Username.Should().Be(toCreate.Username);
    }}
    
    [Test]
    public void can_NOT_create_user_without_identifier()
    {{
        // Arrange
        var toCreate = new FakeUserForCreationDto().Generate();
        toCreate.Identifier = null;
        var newUser = () => User.Create(toCreate);

        // Act + Assert
        newUser.Should().Throw<FluentValidation.ValidationException>();
    }}

    [Test]
    public void queue_domain_event_on_create()
    {{
        // Arrange + Act
        var fakeRecipe = FakeUser.Generate();

        // Assert
        fakeRecipe.DomainEvents.Count.Should().Be(1);
        fakeRecipe.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(UserCreated));
    }}
}}";
    }

    private static string UpdateFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Users", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "Users", projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", "Users", projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", "User", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {domainEventsClassPath.ClassNamespace};
using {domainPolicyClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using NUnit.Framework;

[Parallelizable]
public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public UpdateUserTests()
    {{
        _faker = new Faker();
    }}
    
    [Test]
    public void can_update_user()
    {{
        // Arrange
        var fakeUser = FakeUser.Generate();
        var updatedUser = new FakeUserForUpdateDto().Generate();
        
        // Act
        fakeUser.Update(updatedUser);

        // Assert
        fakeUser.Identifier.Should().Be(updatedUser.Identifier);
        fakeUser.FirstName.Should().Be(updatedUser.FirstName);
        fakeUser.LastName.Should().Be(updatedUser.LastName);
        fakeUser.Email.Value.Should().Be(updatedUser.Email);
        fakeUser.Username.Should().Be(updatedUser.Username);
    }}
    
    [Test]
    public void can_NOT_update_user_without_identifier()
    {{
        // Arrange
        var fakeUser = FakeUser.Generate();
        var updatedUser = new FakeUserForUpdateDto().Generate();
        updatedUser.Identifier = null;
        var newUser = () => fakeUser.Update(updatedUser);

        // Act + Assert
        newUser.Should().Throw<FluentValidation.ValidationException>();
    }}
    
    [Test]
    public void queue_domain_event_on_update()
    {{
        // Arrange
        var fakeUser = FakeUser.Generate();
        var updatedUser = new FakeUserForUpdateDto().Generate();
        fakeUser.DomainEvents.Clear();
        
        // Act
        fakeUser.Update(updatedUser);

        // Assert
        fakeUser.DomainEvents.Count.Should().Be(1);
        fakeUser.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(UserUpdated));
    }}
}}";
    }
}
