namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class GetRecordQueryTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(solutionDirectory, $"{entity.Name}QueryTests.cs", entity.Name, projectBaseName);

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
            var featureName = Utilities.GetEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var queryName = Utilities.QueryRecordName(entity.Name);

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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using {featuresClassPath.ClassNamespace};
    using static {testFixtureName};

    public class {queryName}Tests : TestBase
    {{
        {GetTest(queryName, entity, featureName)}{GetWithoutKeyTest(queryName, entity, featureName)}
    }}
}}";
        }

        private static string GetTest(string queryName, Entity entity, string featureName)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
            var pkName = entity.PrimaryKeyProperty.Name;

            return $@"[Test]
        public async Task {queryName}_Returns_Resource_With_Accurate_Props()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            await InsertAsync({fakeEntityVariableName});

            // Act
            var query = new {featureName}.{queryName}({fakeEntityVariableName}.{pkName});
            var {lowercaseEntityPluralName} = await SendAsync(query);

            // Assert
            {lowercaseEntityPluralName}.Should().BeEquivalentTo({fakeEntityVariableName}, options =>
                options.ExcludingMissingMembers());
        }}";
        }

        private static string GetWithoutKeyTest(string queryName, Entity entity, string featureName)
        {
            var badId = Utilities.GetRandomId(entity.PrimaryKeyProperty.Type);

            return badId == "" ? "" : $@"

        [Test]
        public async Task {queryName}_Throws_KeyNotFoundException_When_Record_Does_Not_Exist()
        {{
            // Arrange
            var badId = {badId};

            // Act
            var query = new {featureName}.{queryName}(badId);
            Func<Task> act = () => SendAsync(query);

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }}";
        }
    }
}