namespace Craftsman.Builders.Tests.IntegrationTests.RolePermissions;

using Craftsman.Builders.Tests.IntegrationTests.Services;
using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class GetRecordQueryTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetRecordQueryTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{entity.Name}QueryTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.GetEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var queryName = FileNames.QueryRecordName();

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetTest(queryName, entity, featureName)}{GetWithoutKeyTest(queryName, entity, featureName)}
}}";
    }

    private static string GetTest(string queryName, Entity entity, string featureName)
    {
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        var fakeParent = IntegrationTestServices.FakeParentTestHelpersForBuilders(entity, out var fakeParentIdRuleFor);

        return $@"[Fact]
    public async Task can_get_existing_{entity.Name.ToLower()}_with_accurate_props()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        {fakeParent}var {fakeEntityVariableName} = new {FileNames.FakeBuilderName(entity.Name)}(){fakeParentIdRuleFor}.Build();
        await testingServiceScope.InsertAsync({fakeEntityVariableName});

        // Act
        var query = new {featureName}.{queryName}({fakeEntityVariableName}.{pkName});
        var {lowercaseEntityName} = await testingServiceScope.SendAsync(query);

        // Assert
        {lowercaseEntityName}.Permission.Should().Be({fakeEntityVariableName}.Permission);
        {lowercaseEntityName}.Role.Should().Be({fakeEntityVariableName}.Role.Value);
    }}";
    }

    private static string GetWithoutKeyTest(string queryName, Entity entity, string featureName)
    {
        var badId = IntegrationTestServices.GetRandomId(Entity.PrimaryKeyProperty.Type);

        return badId == "" ? "" : $@"

    [Fact]
    public async Task get_{entity.Name.ToLower()}_throws_notfound_exception_when_record_does_not_exist()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var badId = {badId};

        // Act
        var query = new {featureName}.{queryName}(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
    }
}
