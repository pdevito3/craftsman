namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class UpdateEntityUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public UpdateEntityUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string entityName, string entityPlural, List<EntityProperty> properties, string projectBaseName, bool overwrite = false )
    {
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"{FileNames.UpdateEntityUnitTestName(entityName)}.cs", entityPlural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, srcDirectory, classPath, entityName, entityPlural, properties, projectBaseName);
        _utilities.CreateFile(classPath, fileText, overwrite);
    }

    private static string WriteTestFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string entityName, string entityPlural, List<EntityProperty> properties, string projectBaseName)
    {
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entityName, projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var updateDto = FileNames.GetDtoName(entityName, Dto.Update);
        var fakeEntityForUpdate = $"Fake{updateDto}";

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {domainEventsClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

[Parallelizable]
public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        _faker = new Faker();
    }}
    
    [Test]
    public void can_update_{entityName.LowercaseFirstLetter()}()
    {{
        // Arrange
        var fake{entityName} = Fake{entityName}.Generate();
        var updated{entityName} = new {fakeEntityForUpdate}().Generate();
        
        // Act
        fake{entityName}.Update(updated{entityName});

        // Assert{GetAssertions(properties, entityName)}
    }}
    
    [Test]
    public void queue_domain_event_on_update()
    {{
        // Arrange
        var fake{entityName} = Fake{entityName}.Generate();
        var updated{entityName} = new {fakeEntityForUpdate}().Generate();
        fake{entityName}.DomainEvents.Clear();
        
        // Act
        fake{entityName}.Update(updated{entityName});

        // Assert
        fake{entityName}.DomainEvents.Count.Should().Be(1);
        fake{entityName}.DomainEvents.FirstOrDefault().Should().BeOfType(typeof({FileNames.EntityUpdatedDomainMessage(entityName)}));
    }}
}}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string entityName)
    {
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType))
        {
            entityAssertions += entityProperty.Type switch
            {
                "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                    $@"{Environment.NewLine}        fake{entityName}.{entityProperty.Name}.Should().BeCloseTo(updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                "DateTime?" =>
                    $@"{Environment.NewLine}        fake{entityName}.{entityProperty.Name}.Should().BeCloseTo((DateTime)updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                "DateTimeOffset?" =>
                    $@"{Environment.NewLine}        fake{entityName}.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset)updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                "TimeOnly?" =>
                    $@"{Environment.NewLine}        fake{entityName}.{entityProperty.Name}.Should().BeCloseTo((TimeOnly)updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                _ =>
                    $@"{Environment.NewLine}        fake{entityName}.{entityProperty.Name}.Should().Be(updated{entityName}.{entityProperty.Name});"
            };
        }

        return entityAssertions;
    }
}
