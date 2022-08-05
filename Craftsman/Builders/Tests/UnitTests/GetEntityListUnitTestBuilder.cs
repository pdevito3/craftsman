namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain.Enums;
using Helpers;
using Services;

public class GetEntityListUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetEntityListUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string entityName, string entityPlural, string projectBaseName, bool isProtected)
    {
        var classPath = ClassPathHelper.UnitTestEntityFeaturesTestsClassPath(testDirectory, $"{FileNames.GetEntityListUnitTestName(entityName)}.cs", entityPlural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, srcDirectory, classPath, entityName, entityPlural, projectBaseName, isProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string entityName, string entityPlural, string projectBaseName, bool isProtected)
    {
        var fakeEntityClass = $"Fake{entityName}";
        var fakeVariableBase = $"fake{entityName}";
        var repoVar = $"_{entityName.LowercaseFirstLetter()}Repository";
        var repoInterface = FileNames.EntityRepositoryInterface(entityName);
        var listVar = $"{entityName.LowercaseFirstLetter()}";
        var paramDto = FileNames.GetDtoName(entityName, Dto.ReadParamaters);
        var featureClassName = FileNames.GetEntityListFeatureClassName(entityName);
        var queryListName = FileNames.QueryListName(entityName);
        var profileName = FileNames.GetMappingName(entityName);
        
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
        
        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtosClassPath.ClassNamespace};
using {profileClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using AutoMapper;
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
    private readonly Mapper _mapper = new Mapper(new MapperConfiguration(cfg => {{ cfg.AddProfile<{profileName}>(); }}));
    private readonly Mock<{repoInterface}> {repoVar};{heimGuardMockProp}

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        {repoVar} = new Mock<{repoInterface}>();
        var sieveOptions = Options.Create(new SieveOptions());
        _sieveProcessor = new SieveProcessor(sieveOptions);{heimGuardMockSetup}
    }}
    
    [Test]
    public async Task cam_get_paged_list_of_{entityName.LowercaseFirstLetter()}()
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
    }}
}}";
    }
}
