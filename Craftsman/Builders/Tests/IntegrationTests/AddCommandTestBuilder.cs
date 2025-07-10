namespace Craftsman.Builders.Tests.IntegrationTests;

using System;
using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;

public static class AddCommandTestBuilder
{
    public sealed record Command(Entity Entity, string Permission, bool FeatureIsProtected) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(scaffoldingDirectoryStore.TestDirectory, $"Add{request.Entity.Name}CommandTests.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = WriteTestFileText(scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, request.Entity, scaffoldingDirectoryStore.ProjectBaseName, request.Permission, request.FeatureIsProtected);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath,
        Entity entity, string projectBaseName, string permission, bool featureIsProtected)
    {
        var featureName = FileNames.AddEntityFeatureClassName(entity.Name);
        var commandName = FileNames.CommandAddName();

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var permissionTest = !featureIsProtected ? null : GetPermissionTest(commandName, entity, featureName, permission);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using {featuresClassPath.ClassNamespace};

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetAddCommandTest(commandName, entity, featureName)}{permissionTest}
}}";
    }

    private static string GetAddCommandTest(string commandName, Entity entity, string featureName)
    {
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();

        return $@"[Fact]
    public async Task can_add_new_{entity.Name.ToLowerInvariant()}_to_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {fakeEntityVariableName} = new {fakeCreationDto}().Generate();

        // Act
        var command = new {featureName}.{commandName}({fakeEntityVariableName});
        var {lowercaseEntityName}Returned = await testingServiceScope.SendAsync(command);
        var {lowercaseEntityName}Created = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {lowercaseEntityName}Returned.Id));

        // Assert{GetAssertions(entity.Properties, lowercaseEntityName, fakeEntityVariableName)}
    }}";
    }

    private static string GetPermissionTest(string commandName, Entity entity, string featureName, string permission)
    {
        if(string.IsNullOrWhiteSpace(permission))
            return null;
        
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";

        return $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});
        var {fakeEntityVariableName} = new {fakeCreationDto}();

        // Act
        var command = new {featureName}.{commandName}({fakeEntityVariableName});
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string lowercaseEntityName, string fakeEntityVariableName)
    {
        var dtoAssertions = "";
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType && x.GetDbRelationship.IsNone && x.CanManipulate))
        {
            switch (entityProperty.Type)
            {
                case "DateTime" or "DateTimeOffset" or "TimeOnly":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeCloseTo({fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeCloseTo({fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    break;
                case "DateTime?":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeCloseTo((DateTime){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeCloseTo((DateTime){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    break;
                case "DateTimeOffset?":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    break;
                case "TimeOnly?":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeCloseTo((TimeOnly){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeCloseTo((TimeOnly){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());";
                    break;
                case "decimal":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeApproximately({fakeEntityVariableName}.{entityProperty.Name}, 0.001M);";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeApproximately({fakeEntityVariableName}.{entityProperty.Name}, 0.001M);";
                    break;
                case "decimal?":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeApproximately((decimal){fakeEntityVariableName}.{entityProperty.Name}, 0.001M);";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeApproximately((decimal){fakeEntityVariableName}.{entityProperty.Name}, 0.001M);";
                    break;
                case "float":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeApproximately({fakeEntityVariableName}.{entityProperty.Name}, 0.001F);";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeApproximately({fakeEntityVariableName}.{entityProperty.Name}, 0.001F);";
                    break;
                case "float?":
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeApproximately((float){fakeEntityVariableName}.{entityProperty.Name}, 0.001F);";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeApproximately((float){fakeEntityVariableName}.{entityProperty.Name}, 0.001F);";
                    break;
                default:
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().Be({fakeEntityVariableName}.{entityProperty.Name});";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().Be({fakeEntityVariableName}.{entityProperty.Name});";
                    break;
            }
        }
        foreach (var entityProperty in properties.Where(x => x.IsStringArray && x.CanManipulate))
        {
            dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().BeEquivalentTo({fakeEntityVariableName}.{entityProperty.Name});";
            entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().BeEquivalentTo({fakeEntityVariableName}.{entityProperty.Name});";
        }
        foreach (var entityProperty in properties.Where(x => x.IsValueObject && x.CanManipulate))
        {
            dtoAssertions += entityProperty.ValueObjectType.GetIntegratedCreationReturnedAssertion($"{lowercaseEntityName}Returned", entityProperty.Name, fakeEntityVariableName, entityProperty.Type);
            entityAssertions += entityProperty.ValueObjectType.GetIntegratedCreationDomainAssertion($"{lowercaseEntityName}Created", entityProperty.Name, fakeEntityVariableName, entityProperty.Type);
        }

        return string.Join(Environment.NewLine, dtoAssertions, entityAssertions);
    }
}