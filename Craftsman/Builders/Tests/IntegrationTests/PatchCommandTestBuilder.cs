namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Linq;
    using System.Text;

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

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "", projectBaseName);
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
    using {exceptionClassPath.ClassNamespace};
    using {featuresClassPath.ClassNamespace};
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using static {testFixtureName};

    public class {commandName}Tests : TestBase
    {{
        {GetAddCommandTest(commandName, entity, featureName, lookupVal, myProp)}
        {BadKey(commandName, entity, featureName)}{NullPatchDoc(commandName, entity, featureName)}
    }}
}}";
        }

        private static string GetAddCommandTest(string commandName, Entity entity, string featureName, string lookupVal, EntityProperty prop)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var pkName = Entity.PrimaryKeyProperty.Name;
            var lowercaseEntityPk = pkName.LowercaseFirstLetter();

            return $@"[Test]
        public async Task can_patch_existing_{entity.Name.ToLower()}_in_db()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            await InsertAsync({fakeEntityVariableName});
            var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());
            var {lowercaseEntityPk} = {lowercaseEntityName}.{pkName};

            var patchDoc = new JsonPatchDocument<{updateDto}>();
            var newValue = {lookupVal};
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{prop.Name}, newValue);

            // Act
            var command = new {featureName}.{commandName}({lowercaseEntityPk}, patchDoc);
            await SendAsync(command);
            var updated{entity.Name} = await ExecuteDbContextAsync(db => db.{entity.Plural}.Where({entity.Lambda} => {entity.Lambda}.{pkName} == {lowercaseEntityPk}).SingleOrDefaultAsync());

            // Assert
            updated{entity.Name}.{prop.Name}.Should().Be(newValue);
        }}";
        }

        private static string NullPatchDoc(string commandName, Entity entity, string featureName)
        {
            var randomId = Utilities.GetRandomId(Entity.PrimaryKeyProperty.Type);

            return randomId == "" ? "" : $@"

        [Test]
        public async Task passing_null_patchdoc_throws_apiexception()
        {{
            // Arrange
            var randomId = {randomId};

            // Act
            var command = new {featureName}.{commandName}(randomId, null);
            Func<Task> act = () => SendAsync(command);

            // Assert
            act.Should().Throw<ApiException>();
        }}";
        }

        private static string BadKey(string commandName, Entity entity, string featureName)
        {
            var badId = Utilities.GetRandomId(Entity.PrimaryKeyProperty.Type);
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);

            return badId == "" ? "" : $@"
        [Test]
        public async Task patch_{entity.Name.ToLower()}_throws_keynotfound_exception_when_record_does_not_exist()
        {{
            // Arrange
            var badId = {badId};
            var patchDoc = new JsonPatchDocument<{updateDto}>();

            // Act
            var command = new {featureName}.{commandName}(badId, patchDoc);
            Func<Task> act = () => SendAsync(command);

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }}";
        }
    }
}