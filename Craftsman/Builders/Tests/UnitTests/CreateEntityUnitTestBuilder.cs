namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain.Enums;
using Helpers;
using Services;

public class CreateEntityUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CreateEntityUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string entityName, string entityPlural, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"{FileNames.CreateEntityUnitTestName(entityName)}.cs", entityPlural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, srcDirectory, classPath, entityName, entityPlural, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string entityName, string entityPlural, string projectBaseName)
    {
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entityName, projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", entityPlural, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {domainEventsClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using Xunit;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker = new Faker();
    
    [Fact]
    public void can_create_valid_{entityName.LowercaseFirstLetter()}()
    {{
        // Arrange + Act
        var fake{entityName} = Fake{entityName}.Generate();

        // Assert
        fake{entityName}.Should().NotBeNull();
    }}

    [Fact]
    public void queue_domain_event_on_create()
    {{
        // Arrange + Act
        var fake{entityName} = Fake{entityName}.Generate();

        // Assert
        fake{entityName}.DomainEvents.Count.Should().Be(1);
        fake{entityName}.DomainEvents.FirstOrDefault().Should().BeOfType(typeof({FileNames.EntityCreatedDomainMessage(entityName)}));
    }}
}}";
    }
}
