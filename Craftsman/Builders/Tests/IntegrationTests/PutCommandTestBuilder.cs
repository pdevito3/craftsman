namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class PutCommandTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Update{entity.Name}CommandTests.cs", entity.Name, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
        {
            var featureName = Utilities.UpdateEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = Utilities.CommandUpdateName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeUpdateDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Update));
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var pkName = Entity.PrimaryKeyProperty.Name;
            var lowercaseEntityPk = pkName.LowercaseFirstLetter();

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(testDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, projectBaseName);

            var fakeParent = Utilities.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);
            var foreignEntityUsings = Utilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);
            
            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using static {testFixtureName};{foreignEntityUsings}

public class {commandName}Tests : TestBase
{{
    [Test]
    public async Task can_update_existing_{entity.Name.ToLower()}_in_db()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
        var updated{entity.Name}Dto = new {fakeUpdateDto}(){fakeParentIdRuleFor}.Generate();
        await InsertAsync({fakeEntityVariableName});

        var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());
        var {lowercaseEntityPk} = {lowercaseEntityName}.{pkName};

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityPk}, updated{entity.Name}Dto);
        await SendAsync(command);
        var updated{entity.Name} = await ExecuteDbContextAsync(db => db.{entity.Plural}.Where({entity.Lambda} => {entity.Lambda}.{pkName} == {lowercaseEntityPk}).SingleOrDefaultAsync());

        // Assert
        updated{entity.Name}.Should().BeEquivalentTo(updated{entity.Name}Dto, options =>
            options.ExcludingMissingMembers());
    }}{GetUpdateWithoutKeyTest(commandName, entity, featureName)}
}}";
        }

        private static string GetUpdateWithoutKeyTest(string commandName, Entity entity, string featureName)
        {
            var badId = Utilities.GetRandomId(Entity.PrimaryKeyProperty.Type);
            var fakeUpdateDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Update));
            Utilities.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);

            return badId == "" ? "" : $@"

    [Test]
    public async Task update_{entity.Name.ToLower()}_throws_notfoundexception_when_record_does_not_exist()
    {{
        // Arrange
        var badId = {badId};
        var updated{entity.Name}Dto = new {fakeUpdateDto}(){fakeParentIdRuleFor}.Generate();

        // Act
        var command = new {featureName}.{commandName}(badId, updated{entity.Name}Dto);
        Func<Task> act = () => SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
        }
    }
}