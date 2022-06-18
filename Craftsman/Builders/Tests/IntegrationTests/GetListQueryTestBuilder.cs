namespace Craftsman.Builders.Tests.IntegrationTests;

using System;
using System.IO;
using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetListQueryTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetListQueryTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{entity.Name}ListQueryTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(testDirectory, srcDirectory, classPath, entity, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.GetEntityListFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();

        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(testDirectory, featureName, entity.Plural, projectBaseName);

        var sortTests = "";
        var filterTests = "";

        foreach (var prop in entity.Properties.Where(e => e.CanSort && e.Type != "Guid").ToList())
        {
            sortTests += GetEntitiesListSortedInAscOrder(entity, prop);
            sortTests += GetEntitiesListSortedInDescOrder(entity, prop);
        }

        foreach (var prop in entity.Properties.Where(e => e.CanFilter).ToList())
            filterTests += GetEntitiesListFiltered(entity, prop);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {dtoClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {exceptionClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;
using static {testFixtureName};{foreignEntityUsings}

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {GetEntitiesTest(entity)}
    {filterTests}
}}";
    }

    private static string GetEntitiesTest(Entity entity)
    {
        var queryName = FileNames.QueryListName(entity.Name);
        var fakeEntity = FileNames.FakerName(entity.Name);
        var entityParams = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var fakeParent = IntegrationTestServices.FakeParentTestHelpersTwoCount(entity, out var fakeParentIdRuleForOne, out var fakeParentIdRuleForTwo);
        return @$"
    [Fact]
    public async Task can_get_{entity.Name.ToLower()}_list()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableNameOne} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleForOne}.Generate());
        var {fakeEntityVariableNameTwo} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleForTwo}.Generate());
        var queryParameters = new {entityParams}();

        await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        // Act
        var query = new {FileNames.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
        var {lowercaseEntityPluralName} = await SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}.Count.Should().BeGreaterThanOrEqualTo(2);
    }}";
    }

    private static string GetEntitiesListSortedInAscOrder(Entity entity, EntityProperty prop)
    {
        var queryName = FileNames.QueryListName(entity.Name);
        var fakeEntity = FileNames.FakerName(entity.Name);
        var entityParams = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var alpha = @$"""alpha""";
        var bravo = @$"""bravo""";

        if (prop.Type == "string")
        {
            // leave variables as is
        }
        else if (prop.Type == "Guid")
        {
            alpha = "Guid.NewGuid()";
            bravo = "Guid.NewGuid()";
        }
        else if (prop.Type.Contains("int"))
        {
            alpha = "1";
            bravo = "2";
        }
        else if (prop.Type.Contains("DateTime"))
        {
            alpha = "DateTime.Now.AddDays(1)";
            bravo = "DateTime.Now.AddDays(2)";
        }
        else
        {
            //no tests generated for other types at this time
            return "";
        }

        var fakeParent = IntegrationTestServices.FakeParentTestHelpersTwoCount(entity, out var fakeParentIdRuleForOne, out var fakeParentIdRuleForTwo);
        return $@"
    [Fact]
    public async Task can_get_sorted_list_of_{entity.Name.ToLower()}_by_{prop.Name}_in_asc_order()
    {{
        //Arrange
        {fakeParent}var {fakeEntityVariableNameOne} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entity.Lambda} => {entity.Lambda}.{prop.Name}, _ => {bravo}){fakeParentIdRuleForOne.RemoveLastNewLine()}
            .Generate());
        var {fakeEntityVariableNameTwo} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entity.Lambda} => {entity.Lambda}.{prop.Name}, _ => {alpha}){fakeParentIdRuleForTwo.RemoveLastNewLine()}
            .Generate());
        var queryParameters = new {entityParams}() {{ SortOrder = ""{prop.Name}"" }};

        await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        //Act
        var query = new {FileNames.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
        var {lowercaseEntityPluralName} = await SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}
            .FirstOrDefault()
            .Should().BeEquivalentTo(fake{entity.Name}Two, options =>
                options.ExcludingMissingMembers());
        {lowercaseEntityPluralName}
            .Skip(1)
            .FirstOrDefault()
            .Should().BeEquivalentTo(fake{entity.Name}One, options =>
                options.ExcludingMissingMembers());
    }}{Environment.NewLine}";
    }

    private static string GetEntitiesListSortedInDescOrder(Entity entity, EntityProperty prop)
    {
        var queryName = FileNames.QueryListName(entity.Name);
        var fakeEntity = FileNames.FakerName(entity.Name);
        var entityParams = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var alpha = @$"""alpha""";
        var bravo = @$"""bravo""";

        if (prop.Type == "string")
        {
            // leave variables as is
        }
        else if (prop.Type == "Guid")
        {
            alpha = "Guid.NewGuid()";
            bravo = "Guid.NewGuid()";
        }
        else if (prop.Type.Contains("int"))
        {
            alpha = "1";
            bravo = "2";
        }
        else if (prop.Type.Contains("DateTime"))
        {
            alpha = "DateTime.Now.AddDays(1)";
            bravo = "DateTime.Now.AddDays(2)";
        }
        else
        {
            //no tests generated for other types at this time
            return "";
        }

        var fakeParent = IntegrationTestServices.FakeParentTestHelpersTwoCount(entity, out var fakeParentIdRuleForOne, out var fakeParentIdRuleForTwo);
        return $@"
    [Fact]
    public async Task can_get_sorted_list_of_{entity.Name.ToLower()}_by_{prop.Name}_in_desc_order()
    {{
        //Arrange
        {fakeParent}var {fakeEntityVariableNameOne} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entity.Lambda} => {entity.Lambda}.{prop.Name}, _ => {alpha}){fakeParentIdRuleForOne.RemoveLastNewLine()}
            .Generate());
        var {fakeEntityVariableNameTwo} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entity.Lambda} => {entity.Lambda}.{prop.Name}, _ => {bravo}){fakeParentIdRuleForTwo.RemoveLastNewLine()}
            .Generate());
        var queryParameters = new {entityParams}() {{ SortOrder = ""-{prop.Name}"" }};

        await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        //Act
        var query = new {FileNames.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
        var {lowercaseEntityPluralName} = await SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}
            .FirstOrDefault()
            .Should().BeEquivalentTo(fake{entity.Name}Two, options =>
                options.ExcludingMissingMembers());
        {lowercaseEntityPluralName}
            .Skip(1)
            .FirstOrDefault()
            .Should().BeEquivalentTo(fake{entity.Name}One, options =>
                options.ExcludingMissingMembers());
    }}{Environment.NewLine}";
    }

    private static string GetEntitiesListFiltered(Entity entity, EntityProperty prop)
    {
        var queryName = FileNames.QueryListName(entity.Name);
        var fakeEntity = FileNames.FakerName(entity.Name);
        var entityParams = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var expectedFilterableProperty = @$"fake{entity.Name}Two.{prop.Name}";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var alpha = @$"""alpha""";
        var bravo = @$"""bravo""";
        var bravoFilterVal = "bravo";

        if (prop.IsSmartEnum())
            return "";

        if (prop.Type == "string")
        {
            // leave variables as is
        }
        else if (prop.Type == "Guid")
        {
            alpha = "Guid.NewGuid()";
            bravo = "Guid.NewGuid()";
        }
        else if (prop.Type.Contains("int"))
        {
            alpha = "1";
            bravo = "2";
            bravoFilterVal = bravo;
        }
        else if (prop.Type.Contains("DateTime"))
        {
            alpha = "DateTime.Now.AddDays(1)";
            bravo = @$"DateTime.Parse(DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy""))"; // filter by date like this because it needs to be an exact match (in this case)
            bravoFilterVal = @$"{{DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy"")}}";
        }
        else if (prop.Type.Contains("bool"))
        {
            alpha = "false";
            bravo = "true";
            bravoFilterVal = bravo;
        }
        else
        {
            //no tests generated for other types at this time
            return "";
        }

        var fakeParent = IntegrationTestServices.FakeParentTestHelpersTwoCount(entity, out var fakeParentIdRuleForOne, out var fakeParentIdRuleForTwo);
        return $@"
    [Fact]
    public async Task can_filter_{entity.Name.ToLower()}_list_using_{prop.Name}()
    {{
        //Arrange
        {fakeParent}var {fakeEntityVariableNameOne} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entity.Lambda} => {entity.Lambda}.{prop.Name}, _ => {alpha}){fakeParentIdRuleForOne.RemoveLastNewLine()}
            .Generate());
        var {fakeEntityVariableNameTwo} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entity.Lambda} => {entity.Lambda}.{prop.Name}, _ => {bravo}){fakeParentIdRuleForTwo.RemoveLastNewLine()}
            .Generate());
        var queryParameters = new {entityParams}() {{ Filters = $""{prop.Name} == {{{expectedFilterableProperty}}}"" }};

        await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        //Act
        var query = new {FileNames.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
        var {lowercaseEntityPluralName} = await SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}.Should().HaveCount(1);
        {lowercaseEntityPluralName}
            .FirstOrDefault()
            .Should().BeEquivalentTo(fake{entity.Name}Two, options =>
                options.ExcludingMissingMembers());
    }}{Environment.NewLine}";
    }
}
