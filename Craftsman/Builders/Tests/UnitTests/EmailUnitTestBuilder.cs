namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class EmailUnitTestBuilder
{
    public sealed record Command(string EntityName, string EntityPlural) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(scaffoldingDirectoryStore.TestDirectory, $"{FileNames.CreateEntityUnitTestName(request.EntityName)}.cs", request.EntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = WriteTestFileText(scaffoldingDirectoryStore.SrcDirectory, classPath, request.EntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string srcDirectory, ClassPath classPath, string entityPlural, string projectBaseName)
    {
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {entityClassPath.ClassNamespace};
using Bogus;

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
