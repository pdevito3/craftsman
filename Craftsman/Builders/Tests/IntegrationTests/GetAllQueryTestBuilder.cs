namespace Craftsman.Builders.Tests.IntegrationTests;

using System;
using System.IO;
using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class GetAllQueryTestBuilder
{
    public sealed record Command(Entity Entity, string Permission, bool FeatureIsProtected) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(scaffoldingDirectoryStore.TestDirectory, $"GetAll{request.Entity.Plural}QueryTests.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = WriteTestFileText(scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, request.Entity, scaffoldingDirectoryStore.ProjectBaseName, request.Permission, request.FeatureIsProtected);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath,
        Entity entity, string projectBaseName, string permission, bool featureIsProtected)
    {
        var featureName = FileNames.GetEntityListFeatureClassName(entity.Name);
        var permissionTest = !featureIsProtected ? null : GetPermissionTest(entity.Plural, featureName, permission);

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(testDirectory, featureName, entity.Plural, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {dtoClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Domain;
using System.Threading.Tasks;

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetEntitiesTest(entity)}{permissionTest}
}}";
    }

    private static string GetEntitiesTest(Entity entity)
    {
        var queryName = FileNames.QueryAllName();
        var fakeEntityVariableNameOne = $"{entity.Name.LowercaseFirstLetter()}One";
        var fakeEntityVariableNameTwo = $"{entity.Name.LowercaseFirstLetter()}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();

        
        return @$"
    [Fact]
    public async Task can_get_all_{entity.Plural.ToLowerInvariant()}()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {fakeEntityVariableNameOne} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var {fakeEntityVariableNameTwo} = new {FileNames.FakeBuilderName(entity.Name)}().Build();

        await testingServiceScope.InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        // Act
        var query = new {FileNames.GetAllEntitiesFeatureClassName(entity.Plural)}.{queryName}();
        var {lowercaseEntityPluralName} = await testingServiceScope.SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}.Count.Should().BeGreaterThanOrEqualTo(2);
        {lowercaseEntityPluralName}.FirstOrDefault(x => x.Id == {fakeEntityVariableNameOne}.Id).Should().NotBeNull();
        {lowercaseEntityPluralName}.FirstOrDefault(x => x.Id == {fakeEntityVariableNameTwo}.Id).Should().NotBeNull();
    }}";
    }
    
    private static string GetPermissionTest(string entityPlural, string featureName, string permission)
    {
        if(string.IsNullOrWhiteSpace(permission))
            return null;
        
        var queryName = FileNames.QueryListName();
        return $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});

        // Act
        var query = new {FileNames.GetAllEntitiesFeatureClassName(entityPlural)}.{queryName}();
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }
}