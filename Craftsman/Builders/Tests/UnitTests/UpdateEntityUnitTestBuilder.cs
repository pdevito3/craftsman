namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class UpdateEntityUnitTestBuilder
{
    public sealed record Command(string EntityName, string EntityPlural, List<EntityProperty> Properties) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(scaffoldingDirectoryStore.TestDirectory, $"{FileNames.UpdateEntityUnitTestName(request.EntityName)}.cs", request.EntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = WriteTestFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, request.EntityName, request.EntityPlural, request.Properties, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string entityName, string entityPlural, List<EntityProperty> properties, string projectBaseName)
    {
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entityName, projectBaseName);
        var domainEventsClassPath = ClassPathHelper.DomainEventsClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        
        var updateModelName = EntityModel.Update.GetClassName(entityName);
        var fakeUpdateModelName = FileNames.FakerName(updateModelName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {domainEventsClassPath.ClassNamespace};
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = {exceptionClassPath.ClassNamespace}.ValidationException;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        _faker = new Faker();
    }}
    
    [Fact]
    public void can_update_{entityName.LowercaseFirstLetter()}()
    {{
        // Arrange
        var {entityName.LowercaseFirstLetter()} = new {FileNames.FakeBuilderName(entityName)}().Build();
        var updated{entityName} = new {fakeUpdateModelName}().Generate();
        
        // Act
        {entityName.LowercaseFirstLetter()}.Update(updated{entityName});

        // Assert{GetAssertions(properties, entityName)}
    }}
    
    [Fact]
    public void queue_domain_event_on_update()
    {{
        // Arrange
        var {entityName.LowercaseFirstLetter()} = new {FileNames.FakeBuilderName(entityName)}().Build();
        var updated{entityName} = new {fakeUpdateModelName}().Generate();
        {entityName.LowercaseFirstLetter()}.DomainEvents.Clear();
        
        // Act
        {entityName.LowercaseFirstLetter()}.Update(updated{entityName});

        // Assert
        {entityName.LowercaseFirstLetter()}.DomainEvents.Count.Should().Be(1);
        {entityName.LowercaseFirstLetter()}.DomainEvents.FirstOrDefault().Should().BeOfType(typeof({FileNames.EntityUpdatedDomainMessage(entityName)}));
    }}
}}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string entityName)
    {
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType && x.GetDbRelationship.IsNone && x.CanManipulate))
        {
            entityAssertions += entityProperty.Type switch
            {
                "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                    $@"{Environment.NewLine}        {entityName.LowercaseFirstLetter()}.{entityProperty.Name}.Should().BeCloseTo(updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                "DateTime?" =>
                    $@"{Environment.NewLine}        {entityName.LowercaseFirstLetter()}.{entityProperty.Name}.Should().BeCloseTo((DateTime)updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                "DateTimeOffset?" =>
                    $@"{Environment.NewLine}        {entityName.LowercaseFirstLetter()}.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset)updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                "TimeOnly?" =>
                    $@"{Environment.NewLine}        {entityName.LowercaseFirstLetter()}.{entityProperty.Name}.Should().BeCloseTo((TimeOnly)updated{entityName}.{entityProperty.Name}, 1.Seconds());",
                _ =>
                    $@"{Environment.NewLine}        {entityName.LowercaseFirstLetter()}.{entityProperty.Name}.Should().Be(updated{entityName}.{entityProperty.Name});"
            };
        }
        foreach (var entityProperty in properties.Where(x => x.IsStringArray && x.CanManipulate))
        {
            entityAssertions += $@"{Environment.NewLine}        {entityName.LowercaseFirstLetter()}.{entityProperty.Name}.Should().BeEquivalentTo(updated{entityName}.{entityProperty.Name});";
        }

        return entityAssertions;
    }
}
