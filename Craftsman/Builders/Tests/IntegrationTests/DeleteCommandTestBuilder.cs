namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class DeleteCommandTestBuilder
{
    public sealed record Command(Entity Entity, bool UseSoftDelete, string Permission, bool FeatureIsProtected) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(scaffoldingDirectoryStore.TestDirectory, $"Delete{request.Entity.Name}CommandTests.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = WriteTestFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, request.Entity, scaffoldingDirectoryStore.ProjectBaseName, request.UseSoftDelete, request.Permission, request.FeatureIsProtected);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory,
        ClassPath classPath, Entity entity, string projectBaseName, bool useSoftDelete, string permission,
        bool featureIsProtected)
    {
        var featureName = FileNames.DeleteEntityFeatureClassName(entity.Name);
        var commandName = FileNames.CommandDeleteName();
        var softDeleteTest = useSoftDelete ? SoftDeleteTest(commandName, entity, featureName) : "";

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var permissionTest = !featureIsProtected ? null : GetPermissionTest(commandName, featureName, permission);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetDeleteTest(commandName, entity, featureName)}{GetDeleteWithoutKeyTest(commandName, entity, featureName)}{softDeleteTest}{permissionTest}
}}";
    }

    private static string GetDeleteTest(string commandName, Entity entity, string featureName)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var dbResponseVariableName = $"{lowercaseEntityName}Response";
        var pkName = Entity.PrimaryKeyProperty.Name;


        return $@"[Fact]
    public async Task can_delete_{entity.Name.ToLowerInvariant()}_from_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {lowercaseEntityName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        await testingServiceScope.InsertAsync({lowercaseEntityName});

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityName}.{pkName});
        await testingServiceScope.SendAsync(command);
        var {dbResponseVariableName} = await testingServiceScope
            .ExecuteDbContextAsync(db => db.{entity.Plural}
                .CountAsync({entity.Lambda} => {entity.Lambda}.Id == {lowercaseEntityName}.{pkName}));

        // Assert
        {dbResponseVariableName}.Should().Be(0);
    }}";
    }

    private static string GetDeleteWithoutKeyTest(string commandName, Entity entity, string featureName)
    {
        var badId = IntegrationTestServices.GetRandomId(Entity.PrimaryKeyProperty.Type);

        return badId == "" ? "" : $@"

    [Fact]
    public async Task delete_{entity.Name.ToLowerInvariant()}_throws_notfoundexception_when_record_does_not_exist()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var badId = {badId};

        // Act
        var command = new {featureName}.{commandName}(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
    }

    private static string SoftDeleteTest(string commandName, Entity entity, string featureName)
    {
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        return $@"

    [Fact]
    public async Task can_softdelete_{entity.Name.ToLowerInvariant()}_from_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {lowercaseEntityName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        await testingServiceScope.InsertAsync({lowercaseEntityName});

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityName}.{pkName});
        await testingServiceScope.SendAsync(command);
        var deleted{entity.Name} = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == {lowercaseEntityName}.{pkName}));

        // Assert
        deleted{entity.Name}?.IsDeleted.Should().BeTrue();
    }}";
    }
    
    private static string GetPermissionTest(string commandName, string featureName, string permission)
    {
        return string.IsNullOrWhiteSpace(permission) ? null : $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});

        // Act
        var command = new {featureName}.{commandName}(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }
}