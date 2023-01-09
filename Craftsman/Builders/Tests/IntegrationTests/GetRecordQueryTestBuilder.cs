﻿namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetRecordQueryTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetRecordQueryTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, string projectBaseName, bool overwrite = false)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{entity.Name}QueryTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName);
        _utilities.CreateFile(classPath, fileText, overwrite);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.GetEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var queryName = FileNames.QueryRecordName();

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");
        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using {exceptionsClassPath.ClassNamespace};
using System.Threading.Tasks;
using static {testFixtureName};{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetTest(queryName, entity, featureName)}{GetWithoutKeyTest(queryName, entity, featureName)}
}}";
    }

    private static string GetTest(string queryName, Entity entity, string featureName)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"fake{entity.Name}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        var fakeParent = IntegrationTestServices.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);

        return $@"[Test]
    public async Task can_get_existing_{entity.Name.ToLower()}_with_accurate_props()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
        await InsertAsync({fakeEntityVariableName});

        // Act
        var query = new {featureName}.{queryName}({fakeEntityVariableName}.{pkName});
        var {lowercaseEntityName} = await SendAsync(query);

        // Assert{GetAssertions(entity.Properties, lowercaseEntityName, fakeEntityVariableName)}
    }}";
    }

    private static string GetWithoutKeyTest(string queryName, Entity entity, string featureName)
    {
        var badId = IntegrationTestServices.GetRandomId(Entity.PrimaryKeyProperty.Type);

        return badId == "" ? "" : $@"

    [Test]
    public async Task get_{entity.Name.ToLower()}_throws_notfound_exception_when_record_does_not_exist()
    {{
        // Arrange
        var badId = {badId};

        // Act
        var query = new {featureName}.{queryName}(badId);
        Func<Task> act = () => SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string lowercaseEntityName, string fakeEntityVariableName)
    {
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType))
        {
            entityAssertions += entityProperty.Type switch
            {
                "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                    $@"{Environment.NewLine}        {lowercaseEntityName}.{entityProperty.Name}.Should().BeCloseTo({fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());",
                "DateTime?" =>
                    $@"{Environment.NewLine}        {lowercaseEntityName}.{entityProperty.Name}.Should().BeCloseTo((DateTime){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());",
                "DateTimeOffset?" =>
                    $@"{Environment.NewLine}        {lowercaseEntityName}.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());",
                "TimeOnly?" =>
                    $@"{Environment.NewLine}        {lowercaseEntityName}.{entityProperty.Name}.Should().BeCloseTo((TimeOnly){fakeEntityVariableName}.{entityProperty.Name}, 1.Seconds());",
                _ =>
                    $@"{Environment.NewLine}        {lowercaseEntityName}.{entityProperty.Name}.Should().Be({fakeEntityVariableName}.{entityProperty.Name});"
            };
        }

        return entityAssertions;
    }
}
