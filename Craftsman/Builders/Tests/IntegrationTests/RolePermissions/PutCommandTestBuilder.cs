namespace Craftsman.Builders.Tests.IntegrationTests.RolePermissions;

using Craftsman.Builders.Tests.IntegrationTests.Services;
using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class PutCommandTestBuilder
{
    public sealed record CreateTestsCommand(string SolutionDirectory, string TestDirectory, string SrcDirectory, Entity Entity, string ProjectBaseName) : IRequest;

    public class Handler(ICraftsmanUtilities utilities) : IRequestHandler<CreateTestsCommand>
    {
        public Task Handle(CreateTestsCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(request.TestDirectory, $"Update{request.Entity.Name}CommandTests.cs", request.Entity.Plural, request.ProjectBaseName);
            var fileText = WriteTestFileText(request.SolutionDirectory, request.TestDirectory, request.SrcDirectory, classPath, request.Entity, request.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.UpdateEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var commandName = FileNames.CommandUpdateName();
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;
        var lowercaseEntityPk = pkName.LowercaseFirstLetter();

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    [Fact]
    public async Task can_update_existing_{entity.Name.ToLowerInvariant()}_in_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {fakeEntityVariableName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var updated{entity.Name}Dto = new {fakeUpdateDto}().Generate();
        await testingServiceScope.InsertAsync({fakeEntityVariableName});

        var {lowercaseEntityName} = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {fakeEntityVariableName}.Id));
        var {lowercaseEntityPk} = {lowercaseEntityName}.{pkName};

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityPk}, updated{entity.Name}Dto);
        await testingServiceScope.SendAsync(command);
        var updated{entity.Name} = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}.FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{pkName} == {lowercaseEntityPk}));

        // Assert
        updated{entity.Name}?.Permission.Should().Be(updated{entity.Name}Dto.Permission);
        updated{entity.Name}?.Role.Value.Should().Be(updated{entity.Name}Dto.Role);
    }}
}}";
    }
}
