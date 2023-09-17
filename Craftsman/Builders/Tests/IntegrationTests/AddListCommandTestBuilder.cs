namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;

public class AddListCommandTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AddListCommandTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, Feature feature, string projectBaseName, string permission,
        bool featureIsProtected)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"AddList{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, feature, projectBaseName, permission, featureIsProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory,
        ClassPath classPath, Entity entity, Feature feature, string projectBaseName, string permission,
        bool featureIsProtected)
    {
        var featureName = FileNames.AddEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var commandName = feature.Command;

        var dtoUtilClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var parentFakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", feature.ParentEntity, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);
        var permissionTest = !featureIsProtected ? null : GetPermissionTest(commandName, entity, featureName, permission);

        return @$"namespace {classPath.ClassNamespace};

using {dtoUtilClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {parentFakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetAddListCommandTest(entity, feature)}{permissionTest}
}}";
    }

    private static string GetAddListCommandTest(Entity entity, Feature feature)
    {
        var createDto = FileNames.GetDtoName(entity.Name, Dto.Creation);
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableNameOne = $"{entity.Name.LowercaseFirstLetter()}One";
        var fakeEntityVariableNameTwo = $"{entity.Name.LowercaseFirstLetter()}Two";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var fakeParentEntity = $"{feature.ParentEntity.LowercaseFirstLetter()}";

        return $@"[Fact]
    public async Task can_add_new_{entity.Name.ToLower()}_list_to_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {fakeParentEntity} = new {FileNames.FakeBuilderName(feature.ParentEntity)}().Build();
        await testingServiceScope.InsertAsync({fakeParentEntity});
        var {fakeEntityVariableNameOne} = new {fakeCreationDto}().Generate();
        var {fakeEntityVariableNameTwo} = new {fakeCreationDto}().Generate();

        // Act
        var command = new {feature.Name}.{feature.Command}(new List<{createDto}>() {{{fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo}}}, {fakeParentEntity}.Id);
        var {lowercaseEntityName}Returned = await testingServiceScope.SendAsync(command);
        var firstReturned = {lowercaseEntityName}Returned.FirstOrDefault();
        var secondReturned = {lowercaseEntityName}Returned.Skip(1).FirstOrDefault();

        var {lowercaseEntityName}Db = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .Where(x => x.Id == firstReturned.Id || x.Id == secondReturned.Id)
            .ToListAsync());
        var firstDbRecord = {lowercaseEntityName}Db.FirstOrDefault(x => x.Id == firstReturned.Id);
        var secondDbRecord = {lowercaseEntityName}Db.FirstOrDefault(x => x.Id == secondReturned.Id);

        // Assert{GetAssertions(entity.Properties, fakeEntityVariableNameOne, fakeEntityVariableNameTwo)}

        firstDbRecord.{feature.ParentEntity}.Id.Should().Be({fakeParentEntity}.Id);
        secondDbRecord.{feature.ParentEntity}.Id.Should().Be({fakeParentEntity}.Id);
    }}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string fakeEntityVariableNameOne, string fakeEntityVariableNameTwo)
    {
        var dtoAssertions = "";
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType && x.GetDbRelationship.IsNone && x.CanManipulate))
        {
            switch (entityProperty.Type)
            {
                case "DateTime" or "DateTimeOffset" or "TimeOnly":
                    dtoAssertions += $@"{Environment.NewLine}        firstReturned.{entityProperty.Name}.Should().BeCloseTo({fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondReturned.{entityProperty.Name}.Should().BeCloseTo({fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        firstDbRecord.{entityProperty.Name}.Should().BeCloseTo({fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondDbRecord.{entityProperty.Name}.Should().BeCloseTo({fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    break;
                case "DateTime?":
                    dtoAssertions += $@"{Environment.NewLine}        firstReturned.{entityProperty.Name}.Should().BeCloseTo((DateTime){fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondReturned.{entityProperty.Name}.Should().BeCloseTo((DateTime){fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        firstDbRecord.{entityProperty.Name}.Should().BeCloseTo((DateTime){fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondDbRecord.{entityProperty.Name}.Should().BeCloseTo((DateTime){fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    break;
                case "DateTimeOffset?":
                    dtoAssertions += $@"{Environment.NewLine}        firstReturned.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondReturned.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        firstDbRecord.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondDbRecord.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    break;
                case "TimeOnly?":
                    dtoAssertions += $@"{Environment.NewLine}        firstReturned.{entityProperty.Name}.Should().BeCloseTo((TimeOnly){fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondReturned.{entityProperty.Name}.Should().BeCloseTo((TimeOnly){fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    entityAssertions += $@"{Environment.NewLine}        firstDbRecord.{entityProperty.Name}.Should().BeCloseTo((TimeOnly){fakeEntityVariableNameOne}.{entityProperty.Name}, 1.Seconds());
        secondDbRecord.{entityProperty.Name}.Should().BeCloseTo((TimeOnly){fakeEntityVariableNameTwo}.{entityProperty.Name}, 1.Seconds());";
                    break;
                default:
                    dtoAssertions += $@"{Environment.NewLine}        firstReturned.{entityProperty.Name}.Should().Be({fakeEntityVariableNameOne}.{entityProperty.Name});
        secondReturned.{entityProperty.Name}.Should().Be({fakeEntityVariableNameTwo}.{entityProperty.Name});";
                    entityAssertions += $@"{Environment.NewLine}        firstDbRecord.{entityProperty.Name}.Should().Be({fakeEntityVariableNameOne}.{entityProperty.Name});
        secondDbRecord.{entityProperty.Name}.Should().Be({fakeEntityVariableNameTwo}.{entityProperty.Name});";
                    break;
            }
        }

        return string.Join(Environment.NewLine, dtoAssertions, entityAssertions);
    }

    private static string GetPermissionTest(string commandName, Entity entity, string featureName, string permission)
    {
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}List";

        return $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});
        var {fakeEntityVariableName} = new List<{fakeCreationDto}>();

        // Act
        var command = new {featureName}.{commandName}({fakeEntityVariableName});
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }
}
