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

    public class PatchCommandTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(solutionDirectory, $"Patch{entity.Name}CommandTests.cs", entity.Name, projectBaseName);

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
            var featureName = Utilities.PatchEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = Utilities.CommandPatchName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
            var pkName = entity.PrimaryKeyProperty.Name;
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

            if (myProp == null)
                return "// no patch tests were created";

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
    using static {featuresClassPath.ClassNamespace}.{featureName};
    using static {testFixtureName};

    public class {commandName}Tests : TestBase
    {{
        [Test]
        public async Task {commandName}_Updates_Existing_{entity.Name}_In_Db()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            await InsertAsync({fakeEntityVariableName});
            var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());
            var {lowercaseEntityPk} = {lowercaseEntityName}.{pkName};

            var patchDoc = new JsonPatchDocument<{updateDto}>();
            var newValue = {lookupVal};
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, newValue);

            // Act
            var command = new {commandName}({lowercaseEntityPk}, patchDoc);
            await SendAsync(command);
            var updated{entity.Name} = await ExecuteDbContextAsync(db => db.{entity.Plural}.Where({entity.Lambda} => {entity.Lambda}.{pkName} == {lowercaseEntityPk}).SingleOrDefaultAsync());

            // Assert
            updated{entity.Name}.{myProp.Name}.Should().Be(newValue);
        }}
    }}
}}";
        }
    }
}