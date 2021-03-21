namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class AddCommandTests
    {
        public static void CreateEntityWriteTests(string solutionDirectory, Entity entity, string projectBaseName = "")
        {
            try
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

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, string projectBaseName)
        {
            var featureName = Utilities.AddEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var addCommandName = Utilities.CommandAddName(entity.Name);
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, featureName, entity.Plural, projectBaseName);

            return @$"namespace {classPath.ClassNamespace}
{{
    using {fakerClassPath.ClassNamespace};
    using {testUtilClassPath.ClassNamespace};
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using static {featuresClassPath.ClassNamespace}.{featureName};
    using static {testFixtureName};

    public class {addCommandName}Tests : TestBase
    {{
        [Test]
        public async Task {addCommandName}_Adds_New_{entity.Name}_To_Db()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeCreationDto} {{ }}.Generate();

            // Act
            var command = new {addCommandName}({fakeEntityVariableName});
            var {lowercaseEntityName}Returned = await SendAsync(command);
            var {lowercaseEntityName}Created = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());

            // Assert
            {lowercaseEntityName}Returned.Should().BeEquivalentTo({fakeEntityVariableName}, options =>
                options.ExcludingMissingMembers());
            {lowercaseEntityName}Created.Should().BeEquivalentTo({fakeEntityVariableName}, options =>
                options.ExcludingMissingMembers());
        }}
    }}
}}";
        }
    }
}
