namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class AddCommandTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(solutionDirectory, $"Add{entity.Name}CommandTests.cs", entity.Name, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = WriteTestFileText(solutionDirectory, classPath, entity, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, string projectBaseName)
        {
            var featureName = Utilities.AddEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = Utilities.CommandAddName(entity.Name);

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var exceptionsClassPath = ClassPathHelper.CoreExceptionClassPath(solutionDirectory, "", projectBaseName);
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, featureName, entity.Plural, projectBaseName);
            var conflictConditional = entity.PrimaryKeyProperty.Type.IsGuidPropertyType() ? GetConflictTest(commandName, entity, featureName) : "";
            return @$"namespace {classPath.ClassNamespace}
{{
    using {fakerClassPath.ClassNamespace};
    using {testUtilClassPath.ClassNamespace};
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using {featuresClassPath.ClassNamespace};
    using static {testFixtureName};
    using System;
    using {exceptionsClassPath.ClassNamespace};

    public class {commandName}Tests : TestBase
    {{
        {GetAddCommandTest(commandName, entity, featureName)}{conflictConditional}
    }}
}}";
        }

        private static string GetAddCommandTest(string commandName, Entity entity, string featureName)
        {
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();

            return $@"[Test]
        public async Task {commandName}_Adds_New_{entity.Name}_To_Db()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeCreationDto} {{ }}.Generate();

            // Act
            var command = new {featureName}.{commandName}({fakeEntityVariableName});
            var {lowercaseEntityName}Returned = await SendAsync(command);
            var {lowercaseEntityName}Created = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());

            // Assert
            {lowercaseEntityName}Returned.Should().BeEquivalentTo({fakeEntityVariableName}, options =>
                options.ExcludingMissingMembers());
            {lowercaseEntityName}Created.Should().BeEquivalentTo({fakeEntityVariableName}, options =>
                options.ExcludingMissingMembers());
        }}";
        }

        private static string GetConflictTest(string commandName, Entity entity, string featureName)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var lowercaseFakeEntity = Utilities.FakerName(entity.Name);
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            return $@"

        [Test]
        public async Task {commandName}_Throws_Conflict_When_PK_Guid_Exists()
        {{
            // Arrange
            var {lowercaseFakeEntity} = new {fakeEntity} {{ }}.Generate();
            var conflictRecord = new {fakeCreationDto} {{ }}.Generate();
            conflictRecord.{entity.PrimaryKeyProperty.Name} = {lowercaseFakeEntity}.{entity.PrimaryKeyProperty.Name};

            await InsertAsync({lowercaseFakeEntity});

            // Act
            var command = new {featureName}.{commandName}(conflictRecord);
            Func<Task> act = () => SendAsync(command);

            // Assert
            act.Should().Throw<ConflictException>();
        }}";
        }
    }
}