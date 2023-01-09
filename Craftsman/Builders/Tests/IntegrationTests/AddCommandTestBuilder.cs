namespace Craftsman.Builders.Tests.IntegrationTests;

using System;
using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;

public class AddCommandTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AddCommandTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Add{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(testDirectory, srcDirectory, classPath, entity, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.AddEntityFeatureClassName(entity.Name);
        var commandName = FileNames.CommandAddName();

        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using {featuresClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetAddCommandTest(commandName, entity, featureName)}
}}";
    }

    private static string GetAddCommandTest(string commandName, Entity entity, string featureName)
    {
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"fake{entity.Name}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();

        var fakeParent = "";
        var fakeParentIdRuleFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimitiveType)
            {
                var baseVarName = entityProperty.ForeignEntityName != entity.Name
                    ? $"{entityProperty.ForeignEntityName}"
                    : $"{entityProperty.ForeignEntityName}Parent";
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto = FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent += @$"var fake{baseVarName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await testingServiceScope.InsertAsync(fake{baseVarName}One);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleFor +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{baseVarName}One.Id)";
            }
        }

        return $@"[Test]
    public async Task can_add_new_{entity.Name.ToLower()}_to_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        {fakeParent}var {fakeEntityVariableName} = new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate();

        // Act
        var command = new {featureName}.{commandName}({fakeEntityVariableName});
        var {lowercaseEntityName}Returned = await testingServiceScope.SendAsync(command);
        var {lowercaseEntityName}Created = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {lowercaseEntityName}Returned.Id));

        // Assert{GetAssertions(entity.Properties, lowercaseEntityName, fakeEntityVariableName)}
    }}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string lowercaseEntityName, string fakeEntityVariableName)
    {
        var dtoAssertions = "";
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType))
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
                default:
                    dtoAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Returned.{entityProperty.Name}.Should().Be({fakeEntityVariableName}.{entityProperty.Name});";
                    entityAssertions += $@"{Environment.NewLine}        {lowercaseEntityName}Created.{entityProperty.Name}.Should().Be({fakeEntityVariableName}.{entityProperty.Name});";
                    break;
            }
        }

        return string.Join(Environment.NewLine, dtoAssertions, entityAssertions);
    }
}
