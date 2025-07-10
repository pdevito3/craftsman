namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Helpers;
using Services;
using MediatR;

public static class UserUnitTestBuilder
{
    public sealed record CreateTestsCommand : IRequest;
    public sealed record UpdateTestsCommand : IRequest;

    public class CreateTestsHandler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CreateTestsCommand>
    {
        public Task Handle(CreateTestsCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(scaffoldingDirectoryStore.TestDirectory, $"CreateUserTests.cs", "Users", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = CreateFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public class UpdateTestsHandler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<UpdateTestsCommand>
    {
        public Task Handle(UpdateTestsCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(scaffoldingDirectoryStore.TestDirectory, $"UpdateUserTests.cs", "Users", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = UpdateFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string CreateFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var emailClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Emails", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Users", projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", "Users", projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", "User", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, "User", "Users", null, projectBaseName);
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {domainEventsClassPath.ClassNamespace};
using {emailClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {resourcesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using Bogus;
using ValidationException = {exceptionClassPath.ClassNamespace}.ValidationException;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public CreateUserTests()
    {{
        _faker = new Faker();
    }}
    
    [Fact]
    public void can_create_valid_user()
    {{
        // Arrange
        var toCreate = new FakeUserForCreation().Generate();

        // Act
        var newUser = User.Create(toCreate);
        
        // Assert
        newUser.Identifier.Should().Be(toCreate.Identifier);
        newUser.FirstName.Should().Be(toCreate.FirstName);
        newUser.LastName.Should().Be(toCreate.LastName);
        newUser.Email.Should().Be(new Email(toCreate.Email));
        newUser.Username.Should().Be(toCreate.Username);
    }}
    
    [Fact]
    public void can_NOT_create_user_without_identifier()
    {{
        // Arrange
        var toCreate = new FakeUserForCreation().Generate();
        toCreate.Identifier = null;
        var newUser = () => User.Create(toCreate);

        // Act + Assert
        newUser.Should().Throw<ValidationException>();
    }}
    
    [Fact]
    public void can_NOT_create_user_with_whitespace_identifier()
    {{
        // Arrange
        var toCreate = new FakeUserForCreation().Generate();
        toCreate.Identifier = "" "";
        var newUser = () => User.Create(toCreate);

        // Act + Assert
        newUser.Should().Throw<ValidationException>();
    }}

    [Fact]
    public void queue_domain_event_on_create()
    {{
        // Arrange
        var toCreate = new FakeUserForCreation().Generate();

        // Act
        var newUser = User.Create(toCreate);

        // Assert
        newUser.DomainEvents.Count.Should().Be(1);
        newUser.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(UserCreated));
    }}
}}";
    }

    private static string UpdateFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Users", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, "User", "Users", null, projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", "Users", projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", "User", projectBaseName);
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {domainEventsClassPath.ClassNamespace};
using {domainPolicyClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {resourcesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using Bogus;
using ValidationException = {exceptionClassPath.ClassNamespace}.ValidationException;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public UpdateUserTests()
    {{
        _faker = new Faker();
    }}
    
    [Fact]
    public void can_update_user()
    {{
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
        
        // Act
        fakeUser.Update(updatedUser);

        // Assert
        fakeUser.Identifier.Should().Be(updatedUser.Identifier);
        fakeUser.FirstName.Should().Be(updatedUser.FirstName);
        fakeUser.LastName.Should().Be(updatedUser.LastName);
        fakeUser.Email.Value.Should().Be(updatedUser.Email);
        fakeUser.Username.Should().Be(updatedUser.Username);
    }}
    
    [Fact]
    public void can_NOT_update_user_without_identifier()
    {{
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
        updatedUser.Identifier = null;
        var newUser = () => fakeUser.Update(updatedUser);

        // Act + Assert
        newUser.Should().Throw<ValidationException>();
    }}
    
    [Fact]
    public void can_NOT_update_user_with_whitespace_identifier()
    {{
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
        updatedUser.Identifier = "" "";
        var newUser = () => fakeUser.Update(updatedUser);

        // Act + Assert
        newUser.Should().Throw<ValidationException>();
    }}
    
    [Fact]
    public void queue_domain_event_on_update()
    {{
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
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
