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
        public static void CreateTests(string solutionDirectory, Entity entity, Feature feature, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(solutionDirectory, $"{feature.Command}Tests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, classPath, entity, feature, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, Feature feature, string projectBaseName)
        {
            var featureName = Utilities.AddEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = Utilities.CommandAddName(entity.Name);

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var dtoUtilClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "", projectBaseName);
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var parentFakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", feature.ParentEntity, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, featureName, entity.Plural, projectBaseName);
            return @$"namespace {classPath.ClassNamespace}
{{
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
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using static {testFixtureName};

    public class {commandName}Tests : TestBase
    {{
        {GetAddListCommandTest(entity, feature)}
    }}
}}";
        }

        private static string GetAddListCommandTest(Entity entity, Feature feature)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeCreationDto = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var fakeParentEntity = $"fake{feature.ParentEntity}";

            return $@"[Test]
        public async Task {feature.Command}_Adds_New_{entity.Name}_List_To_Db()
        {{
            // Arrange
            var {fakeParentEntity} = new Fake{feature.ParentEntity} {{ }}.Generate();
            await InsertAsync({fakeParentEntity});
            var {fakeEntityVariableName} = new {fakeCreationDto} {{ }}.Generate();

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