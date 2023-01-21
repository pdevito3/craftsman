namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain.Enums;
using Helpers;
using Services;

public class EmailUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public EmailUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string srcDirectory, string entityName, string entityPlural, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"{FileNames.CreateEntityUnitTestName(entityName)}.cs", entityPlural, projectBaseName);
        var fileText = WriteTestFileText(srcDirectory, classPath, entityPlural, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string srcDirectory, ClassPath classPath, string entityPlural, string projectBaseName)
    {
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {entityClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using Xunit;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        _faker = new Faker();
    }}
    
    [Fact]
    public void can_create_valid_email()
    {{
        var validEmail = _faker.Person.Email;
        var emailVo = new Email(validEmail);
        emailVo.Value.Should().Be(validEmail);
    }}

    [Fact]
    public void can_not_add_invalid_email()
    {{
        var validEmail = _faker.Lorem.Word();
        var act = () => new Email(validEmail);
        act.Should().Throw<FluentValidation.ValidationException>();
    }}

    [Fact]
    public void email_can_be_null()
    {{
        var emailVo = new Email(null);
        emailVo.Value.Should().BeNull();
    }}
}}";
    }
}
