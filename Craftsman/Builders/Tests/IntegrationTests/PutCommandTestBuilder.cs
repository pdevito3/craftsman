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
        public static void CreateTests(string solutionDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(solutionDirectory, $"Update{entity.Name}CommandTests.cs", entity.Name, projectBaseName);

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
            var featureName = Utilities.UpdateEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = Utilities.CommandUpdateName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeUpdateDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Update));
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
            var pkName = Entity.PrimaryKeyProperty.Name;
            var lowercaseEntityPk = pkName.LowercaseFirstLetter();

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, featureName, entity.Plural, projectBaseName);

            var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";

            // if no string properties, do one with an int
            if (myProp == null)
            {
                myProp = entity.Properties.Where(e => e.Type.Contains("int") && e.CanManipulate).FirstOrDefault();
                lookupVal = "999999";
            }

            return @$"namespace {classPath.ClassNamespace}
{{
    using {fakerClassPath.ClassNamespace};
    using {testUtilClassPath.ClassNamespace};
    using {dtoClassPath.ClassNamespace};
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using System.Linq;
    using {featuresClassPath.ClassNamespace};
    using static {testFixtureName};

    public class {commandName}Tests : TestBase
    {{
        [Test]
        public async Task can_update_existing_{entity.Name.ToLower()}_in_db()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            var updated{entity.Name}Dto = new {fakeUpdateDto} {{ }}.Generate();
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
        }}
    }}
}}";
        }
    }
}