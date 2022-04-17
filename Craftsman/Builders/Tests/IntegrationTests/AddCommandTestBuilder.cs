namespace Craftsman.Builders.Tests.IntegrationTests
{
    using System;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class AddCommandTestBuilder
    {
        public static void CreateTests(string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Add{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = WriteTestFileText(testDirectory, srcDirectory, classPath, entity, projectBaseName);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
        {
            var featureName = Utilities.AddEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = Utilities.CommandAddName(entity.Name);

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(testDirectory, projectBaseName, "");
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

            var foreignEntityUsings = Utilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using {featuresClassPath.ClassNamespace};
using static {testFixtureName};
using {exceptionsClassPath.ClassNamespace};{foreignEntityUsings}

public class {commandName}Tests : TestBase
{{
    {GetAddCommandTest(commandName, entity, featureName)}
}}";
        }

        private static string GetAddCommandTest(string commandName, Entity entity, string featureName)
        {
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();

            var fakeParent = "";
            var fakeParentIdRuleFor = "";
            foreach (var entityProperty in entity.Properties)
            {
                if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimativeType)
                {
                    var fakeParentClass = Utilities.FakerName(entityProperty.ForeignEntityName);
                    var fakeParentCreationDto = Utilities.FakerName(Utilities.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                    fakeParent += @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{entityProperty.ForeignEntityName}One);{Environment.NewLine}{Environment.NewLine}        ";
                    fakeParentIdRuleFor +=
                        $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
                }
            }

            return $@"[Test]
    public async Task can_add_new_{entity.Name.ToLower()}_to_db()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate();

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
    }
}