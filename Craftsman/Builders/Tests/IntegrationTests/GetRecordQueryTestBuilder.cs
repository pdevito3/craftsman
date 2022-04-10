namespace Craftsman.Builders.Tests.IntegrationTests
{
    using System;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;
    using Enums;

    public class GetRecordQueryTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{entity.Name}QueryTests.cs", entity.Plural, projectBaseName);

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
            var featureName = Utilities.GetEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var queryName = Utilities.QueryRecordName(entity.Name);

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(testDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");
            var foreignEntityUsings = Utilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using {exceptionsClassPath.ClassNamespace};
using System.Threading.Tasks;
using static {testFixtureName};{foreignEntityUsings}

public class {queryName}Tests : TestBase
{{
    {GetTest(queryName, entity, featureName)}{GetWithoutKeyTest(queryName, entity, featureName)}
}}";
        }

        private static string GetTest(string queryName, Entity entity, string featureName)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
            var pkName = Entity.PrimaryKeyProperty.Name;

            var fakeParent = Utilities.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);
            
            return $@"[Test]
    public async Task can_get_existing_{entity.Name.ToLower()}_with_accurate_props()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
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
            var badId = Utilities.GetRandomId(Entity.PrimaryKeyProperty.Type);

            return badId == "" ? "" : $@"

    [Test]
    public async Task get_{entity.Name.ToLower()}_throws_notfound_exception_when_record_does_not_exist()
    {{
        // Arrange
        var badId = {badId};

        // Act
        var query = new {featureName}.{queryName}(badId);
        Func<Task> act = () => SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
        }
    }
}