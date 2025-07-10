namespace Craftsman.Builders.Tests.IntegrationTests.Users;

using System;
using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class AddCommandTestBuilder
{
    public sealed record CreateTestsCommand(string TestDirectory, string SrcDirectory, Entity Entity, string ProjectBaseName) : IRequest;

    public class Handler(ICraftsmanUtilities utilities) : IRequestHandler<CreateTestsCommand>
    {
        public Task Handle(CreateTestsCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(request.TestDirectory, $"Add{request.Entity.Name}CommandTests.cs", request.Entity.Plural, request.ProjectBaseName);
            var fileText = WriteTestFileText(request.TestDirectory, request.SrcDirectory, classPath, request.Entity, request.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.AddEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var commandName = FileNames.CommandAddName();

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using {featuresClassPath.ClassNamespace};{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetAddCommandTest(commandName, entity, featureName)}
}}";
    }

    private static string GetAddCommandTest(string commandName, Entity entity, string featureName)
    {
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();

        var fakeParent = "";
        var fakeParentIdRuleFor = "";

        return $@"[Fact]
    public async Task can_add_new_{entity.Name.ToLowerInvariant()}_to_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        {fakeParent}var {fakeEntityVariableName} = new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate();

        // Act
        var command = new {featureName}.{commandName}({fakeEntityVariableName});
        var {lowercaseEntityName}Returned = await testingServiceScope.SendAsync(command);
        var {lowercaseEntityName}Created = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {lowercaseEntityName}Returned.Id));

        // Assert
        userReturned.FirstName.Should().Be({fakeEntityVariableName}.FirstName);
        userReturned.LastName.Should().Be({fakeEntityVariableName}.LastName);
        userReturned.Username.Should().Be({fakeEntityVariableName}.Username);
        userReturned.Identifier.Should().Be({fakeEntityVariableName}.Identifier);
        userReturned.Email.Should().Be({fakeEntityVariableName}.Email);

        userCreated?.FirstName.Should().Be({fakeEntityVariableName}.FirstName);
        userCreated?.LastName.Should().Be({fakeEntityVariableName}.LastName);
        userCreated?.Username.Should().Be({fakeEntityVariableName}.Username);
        userCreated?.Identifier.Should().Be({fakeEntityVariableName}.Identifier);
        userCreated?.Email.Value.Should().Be({fakeEntityVariableName}.Email);
    }}";
    }
}
