namespace Craftsman.Builders.Tests.IntegrationTests.Users;

using Craftsman.Builders.Tests.IntegrationTests.Services;
using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class GetRecordQueryTestBuilder
{
    public sealed record CreateTestsCommand(string SolutionDirectory, string TestDirectory, string SrcDirectory, Entity Entity, string ProjectBaseName) : IRequest;

    public class Handler(ICraftsmanUtilities utilities) : IRequestHandler<CreateTestsCommand>
    {
        public Task Handle(CreateTestsCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(request.TestDirectory, $"{request.Entity.Name}QueryTests.cs", request.Entity.Plural, request.ProjectBaseName);
            var fileText = WriteTestFileText(request.SolutionDirectory, request.TestDirectory, request.SrcDirectory, classPath, request.Entity, request.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.GetEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var queryName = FileNames.QueryRecordName();

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetTest(queryName, entity, featureName)}{GetWithoutKeyTest(queryName, entity, featureName)}
}}";
    }

    private static string GetTest(string queryName, Entity entity, string featureName)
    {
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        return $@"[Fact]
    public async Task can_get_existing_{entity.Name.ToLowerInvariant()}_with_accurate_props()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {fakeEntityVariableName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        await testingServiceScope.InsertAsync({fakeEntityVariableName});

        // Act
        var query = new {featureName}.{queryName}({fakeEntityVariableName}.{pkName});
        var {lowercaseEntityName} = await testingServiceScope.SendAsync(query);

        // Assert
        user.FirstName.Should().Be({fakeEntityVariableName}.FirstName);
        user.LastName.Should().Be({fakeEntityVariableName}.LastName);
        user.Username.Should().Be({fakeEntityVariableName}.Username);
        user.Identifier.Should().Be({fakeEntityVariableName}.Identifier);
        user.Email.Should().Be({fakeEntityVariableName}.Email.Value);
    }}";
    }

    private static string GetWithoutKeyTest(string queryName, Entity entity, string featureName)
    {
        var badId = IntegrationTestServices.GetRandomId(Entity.PrimaryKeyProperty.Type);

        return badId == "" ? "" : $@"

    [Fact]
    public async Task get_{entity.Name.ToLowerInvariant()}_throws_notfound_exception_when_record_does_not_exist()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var badId = {badId};

        // Act
        var query = new {featureName}.{queryName}(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
    }
}
