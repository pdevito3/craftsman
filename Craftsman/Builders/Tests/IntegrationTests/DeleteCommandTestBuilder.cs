namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class DeleteCommandTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DeleteCommandTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, string projectBaseName, bool useSoftDelete)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Delete{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName, useSoftDelete);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName, bool useSoftDelete)
    {
        var featureName = FileNames.DeleteEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var commandName = FileNames.CommandDeleteName(entity.Name);
        var softDeleteTest = useSoftDelete ? SoftDeleteTest(commandName, entity, featureName) : "";

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using {exceptionsClassPath.ClassNamespace};
using System.Threading.Tasks;
using static {testFixtureName};{foreignEntityUsings}

public class {commandName}Tests : TestBase
{{
    {GetDeleteTest(commandName, entity, featureName)}{GetDeleteWithoutKeyTest(commandName, entity, featureName)}{softDeleteTest}
}}";
    }

    private static string GetDeleteTest(string commandName, Entity entity, string featureName)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"fake{entity.Name}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var dbResponseVariableName = $"{lowercaseEntityName}Response";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var lowercaseEntityPk = pkName.LowercaseFirstLetter();

        var fakeParent = IntegrationTestServices.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);

        return $@"[Fact]
    public async Task can_delete_{entity.Name.ToLower()}_from_db()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
        await InsertAsync({fakeEntityVariableName});
        var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {fakeEntityVariableName}.Id));

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityName}.{pkName});
        await SendAsync(command);
        var {dbResponseVariableName} = await ExecuteDbContextAsync(db => db.{entity.Plural}.CountAsync({entity.Lambda} => {entity.Lambda}.Id == {lowercaseEntityName}.{pkName}));

        // Assert
        {dbResponseVariableName}.Should().Be(0);
    }}";
    }

    private static string GetDeleteWithoutKeyTest(string commandName, Entity entity, string featureName)
    {
        var badId = IntegrationTestServices.GetRandomId(Entity.PrimaryKeyProperty.Type);

        return badId == "" ? "" : $@"

    [Fact]
    public async Task delete_{entity.Name.ToLower()}_throws_notfoundexception_when_record_does_not_exist()
    {{
        // Arrange
        var badId = {badId};

        // Act
        var command = new {featureName}.{commandName}(badId);
        Func<Task> act = () => SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
    }

    private static string SoftDeleteTest(string commandName, Entity entity, string featureName)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"fake{entity.Name}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;
        var lowercaseEntityPk = pkName.LowercaseFirstLetter();

        var fakeParent = IntegrationTestServices.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);

        return $@"

    [Fact]
    public async Task can_softdelete_{entity.Name.ToLower()}_from_db()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
        await InsertAsync({fakeEntityVariableName});
        var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {fakeEntityVariableName}.Id));

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityName}.{pkName});
        await SendAsync(command);
        var deleted{entity.Name} = await ExecuteDbContextAsync(db => db.{entity.Plural}
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == {lowercaseEntityName}.{pkName}));

        // Assert
        deleted{entity.Name}?.IsDeleted.Should().BeTrue();
    }}";
    }
}
