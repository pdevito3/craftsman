namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class AddListCommandTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, Feature feature, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{feature.Command}Tests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, feature, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, Feature feature, string projectBaseName)
        {
            var featureName = Utilities.AddEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = feature.Command;

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(testDirectory, projectBaseName, "");
            var dtoUtilClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
            var parentFakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", feature.ParentEntity, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
            
            var foreignEntityUsings = Utilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);
            
            return @$"namespace {classPath.ClassNamespace};

using {dtoUtilClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {parentFakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using static {testFixtureName};{foreignEntityUsings}

public class {commandName}Tests : TestBase
{{
    {GetAddListCommandTest(entity, feature)}
}}";
        }

        private static string GetAddListCommandTest(Entity entity, Feature feature)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeCreationDto = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var fakeParentEntity = $"fake{feature.ParentEntity}";
            var fakeParentCreationDto = Utilities.FakerName(Utilities.GetDtoName(feature.ParentEntity, Dto.Creation));

            return $@"[Test]
    public async Task can_add_new_{entity.Name.ToLower()}_list_to_db()
    {{
        // Arrange
        var {fakeParentEntity} = Fake{feature.ParentEntity}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync({fakeParentEntity});
        var {fakeEntityVariableName} = new {fakeCreationDto}().Generate();

        // Act
        var command = new {feature.Name}.{feature.Command}(new List<{createDto}>() {{{fakeEntityVariableName}}}, {fakeParentEntity}.Id);
        var {lowercaseEntityName}Returned = await SendAsync(command);
        var {lowercaseEntityName}Created = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());

        // Assert
        {lowercaseEntityName}Returned.FirstOrDefault().Should().BeEquivalentTo({fakeEntityVariableName}, options =>
            options.ExcludingMissingMembers());
        {lowercaseEntityName}Created.Should().BeEquivalentTo({fakeEntityVariableName}, options =>
            options.ExcludingMissingMembers());
    }}";
        }
    }
}