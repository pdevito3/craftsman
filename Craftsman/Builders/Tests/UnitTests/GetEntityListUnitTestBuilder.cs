namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using IntegrationTests.Services;
using Services;

public class GetEntityListUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetEntityListUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, 
        string testDirectory, 
        string srcDirectory, 
        string entityName, 
        string entityPlural, 
        string entityLambda,
        List<EntityProperty> entityProperties,
        string projectBaseName, 
        bool isProtected)
    {
        var classPath = ClassPathHelper.UnitTestEntityFeaturesTestsClassPath(testDirectory, $"{FileNames.GetEntityListUnitTestName(entityName)}.cs", entityPlural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, srcDirectory, classPath, entityName, entityPlural, entityLambda, entityProperties, projectBaseName, isProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, 
        string srcDirectory, 
        ClassPath classPath, 
        string entityName, 
        string entityPlural, 
        string entityLambda,
        List<EntityProperty> entityProperties,
        string projectBaseName, 
        bool isProtected)
    {
        var fakeEntityClass = $"Fake{entityName}";
        var fakeVariableBase = $"fake{entityName}";
        var repoVar = $"_{entityName.LowercaseFirstLetter()}Repository";
        var repoInterface = FileNames.EntityRepositoryInterface(entityName);
        var listVar = $"{entityName.LowercaseFirstLetter()}";
        var paramDto = FileNames.GetDtoName(entityName, Dto.ReadParamaters);
        var featureClassName = FileNames.GetEntityListFeatureClassName(entityName);
        var queryListName = FileNames.QueryListName(entityName);
        
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var dtosClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var profileClassPath = ClassPathHelper.EntityMappingClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entityName, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var servicesClassPath = ClassPathHelper.EntityServicesClassPath(solutionDirectory, "", entityPlural, projectBaseName);

        var heimGuardUsing = isProtected ? $"{Environment.NewLine}using HeimGuard;" : "";
        var heimGuardMockProp = isProtected ? $"{Environment.NewLine}      private readonly Mock<IHeimGuardClient> _heimGuard;" : "";
        var heimGuardMockSetup = isProtected ? $"{Environment.NewLine}        _heimGuard = new Mock<IHeimGuardClient>();" : "";
        var heimGuardMockObj = isProtected ? $", _heimGuard.Object" : "";

        var sortTests = "";
        var filterTests = "";

        foreach (var prop in entityProperties.Where(e => e.CanSort && e.Type != "Guid" && !e.IsSmartEnum()).ToList())
            sortTests += @$"{Environment.NewLine}{Environment.NewLine}{GetEntitiesListTestForSortedOrder(entityName, 
                entityLambda, 
                prop,
                featureClassName,
                repoVar,
                queryListName,
                heimGuardMockObj)}";
        

        foreach (var prop in entityProperties.Where(e => e.CanFilter && !e.IsSmartEnum()).ToList())
            filterTests += @$"{Environment.NewLine}{Environment.NewLine}{GetEntitiesListFiltered(entityName, 
                entityLambda, 
                prop,
                featureClassName,
                repoVar,
                queryListName,
                heimGuardMockObj)}";
        
        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtosClassPath.ClassNamespace};
using {profileClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using MapsterMapper;
using FluentAssertions;{heimGuardUsing}
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using Sieve.Models;
using Sieve.Services;
using NUnit.Framework;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    
    private readonly SieveProcessor _sieveProcessor;
    private readonly Mapper _mapper = new Mapper();
    private readonly Mock<{repoInterface}> {repoVar};{heimGuardMockProp}

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        {repoVar} = new Mock<{repoInterface}>();
        var sieveOptions = Options.Create(new SieveOptions());
        _sieveProcessor = new SieveProcessor(sieveOptions);{heimGuardMockSetup}
    }}
    
    [Test]
    public async Task can_get_paged_list_of_{entityName.LowercaseFirstLetter()}()
    {{
        //Arrange
        var {fakeVariableBase}One = {fakeEntityClass}.Generate();
        var {fakeVariableBase}Two = {fakeEntityClass}.Generate();
        var {fakeVariableBase}Three = {fakeEntityClass}.Generate();
        var {listVar} = new List<{entityName.UppercaseFirstLetter()}>();
        {listVar}.Add({fakeVariableBase}One);
        {listVar}.Add({fakeVariableBase}Two);
        {listVar}.Add({fakeVariableBase}Three);
        var mockDbData = {listVar}.AsQueryable().BuildMock();
        
        var queryParameters = new {paramDto}() {{ PageSize = 1, PageNumber = 2 }};

        {repoVar}
            .Setup(x => x.Query())
            .Returns(mockDbData);
        
        //Act
        var query = new {featureClassName}.{queryListName}(queryParameters);
        var handler = new {featureClassName}.Handler({repoVar}.Object, _mapper, _sieveProcessor{heimGuardMockObj});
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        response.Should().HaveCount(1);
    }}{filterTests}{sortTests}
}}";
    }

    private static string GetEntitiesListTestForSortedOrder(string entityName, 
        string entityLambda, 
        EntityProperty propToTest,
        string featureClassName,
        string repoVar,
        string queryListName,
        string heimGuardMockObj)
    {
        var fakeEntity = FileNames.FakerName(entityName);
        var entityParams = FileNames.GetDtoName(entityName, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entityName}One";
        var fakeEntityVariableNameTwo = $"fake{entityName}Two";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entityName, Dto.Creation));

        var alpha = @$"""alpha""";
        var bravo = @$"""bravo""";

        if (propToTest.Type == "string")
        {
            // leave variables as is
        }
        else if (propToTest.Type == "Guid")
        {
            alpha = "Guid.NewGuid()";
            bravo = "Guid.NewGuid()";
        }
        else if (propToTest.Type.Contains("int"))
        {
            alpha = "1";
            bravo = "2";
        }
        else if (propToTest.Type.Contains("DateTime"))
        {
            alpha = "DateTime.Now.AddDays(1)";
            bravo = "DateTime.Now.AddDays(2)";
        }
        else
        {
            //no tests generated for other types at this time
            return "";
        }

        return $@"    [Test]
    public async Task can_get_sorted_list_of_{entityName.ToLower()}_by_{propToTest.Name}()
    {{
        //Arrange
        var {fakeEntityVariableNameOne} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entityLambda} => {entityLambda}.{propToTest.Name}, _ => {alpha})
            .Generate());
        var {fakeEntityVariableNameTwo} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entityLambda} => {entityLambda}.{propToTest.Name}, _ => {bravo})
            .Generate());
        var queryParameters = new {entityParams}() {{ SortOrder = ""-{propToTest.Name}"" }};

        var {entityName}List = new List<{entityName}>() {{ {fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo} }};
        var mockDbData = {entityName}List.AsQueryable().BuildMock();

        {repoVar}
            .Setup(x => x.Query())
            .Returns(mockDbData);

        //Act
        var query = new {featureClassName}.{queryListName}(queryParameters);
        var handler = new {featureClassName}.Handler({repoVar}.Object, _mapper, _sieveProcessor{heimGuardMockObj});
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        response.FirstOrDefault()
            .Should().BeEquivalentTo(fake{entityName}Two, options =>
                options.ExcludingMissingMembers());
        response.Skip(1)
            .FirstOrDefault()
            .Should().BeEquivalentTo(fake{entityName}One, options =>
                options.ExcludingMissingMembers());
    }}";
    }

    private static string GetEntitiesListFiltered(string entityName, 
        string entityLambda, 
        EntityProperty propToTest,
        string featureClassName,
        string repoVar,
        string queryListName,
        string heimGuardMockObj)
    {
        var fakeEntity = FileNames.FakerName(entityName);
        var entityParams = FileNames.GetDtoName(entityName, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entityName}One";
        var fakeEntityVariableNameTwo = $"fake{entityName}Two";
        var expectedFilterableProperty = @$"fake{entityName}Two.{propToTest.Name}";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entityName, Dto.Creation));

        var alpha = @$"""alpha""";
        var bravo = @$"""bravo""";
        var bravoFilterVal = "bravo";

        if (propToTest.IsSmartEnum())
            return "";

        if (propToTest.Type == "string")
        {
            // leave variables as is
        }
        else if (propToTest.Type == "Guid")
        {
            alpha = "Guid.NewGuid()";
            bravo = "Guid.NewGuid()";
        }
        else if (propToTest.Type.Contains("int"))
        {
            alpha = "1";
            bravo = "2";
            bravoFilterVal = bravo;
        }
        else if (propToTest.Type.Contains("DateTime"))
        {
            alpha = "DateTime.Now.AddDays(1)";
            bravo = @$"DateTime.Parse(DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy""))"; // filter by date like this because it needs to be an exact match (in this case)
            bravoFilterVal = @$"{{DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy"")}}";
        }
        else if (propToTest.Type.Contains("bool"))
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

        return $@"    [Test]
    public async Task can_filter_{entityName.ToLower()}_list_using_{propToTest.Name}()
    {{
        //Arrange
        var {fakeEntityVariableNameOne} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entityLambda} => {entityLambda}.{propToTest.Name}, _ => {alpha})
            .Generate());
        var {fakeEntityVariableNameTwo} = {fakeEntity}.Generate(new {fakeCreationDto}()
            .RuleFor({entityLambda} => {entityLambda}.{propToTest.Name}, _ => {bravo})
            .Generate());
        var queryParameters = new {entityParams}() {{ Filters = $""{propToTest.Name} == {{{expectedFilterableProperty}}}"" }};

        var {entityName.LowercaseFirstLetter()}List = new List<{entityName}>() {{ {fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo} }};
        var mockDbData = {entityName.LowercaseFirstLetter()}List.AsQueryable().BuildMock();

        {repoVar}
            .Setup(x => x.Query())
            .Returns(mockDbData);

        //Act
        var query = new {featureClassName}.{queryListName}(queryParameters);
        var handler = new {featureClassName}.Handler({repoVar}.Object, _mapper, _sieveProcessor{heimGuardMockObj});
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        response.Should().HaveCount(1);
        response
            .FirstOrDefault()
            .Should().BeEquivalentTo(fake{entityName}Two, options =>
                options.ExcludingMissingMembers());
    }}";
    }
}
