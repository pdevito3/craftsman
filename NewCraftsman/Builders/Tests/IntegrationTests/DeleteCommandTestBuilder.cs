namespace NewCraftsman.Builders.Tests.IntegrationTests
{
    using System.IO;
    using System.Text;

    public class DeleteCommandTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, string projectBaseName, bool useSoftDelete)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Delete{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName, useSoftDelete);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName, bool useSoftDelete)
        {
            var featureName = Utilities.DeleteEntityFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var commandName = Utilities.CommandDeleteName(entity.Name);
            var softDeleteTest = useSoftDelete ? SoftDeleteTest(commandName, entity, featureName) : "";

            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(testDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

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

public class {commandName}Tests : TestBase
{{
    {GetDeleteTest(commandName, entity, featureName)}{GetDeleteWithoutKeyTest(commandName, entity, featureName)}{softDeleteTest}
}}";
        }

        private static string GetDeleteTest(string commandName, Entity entity, string featureName)
        {
            var fakeEntity = FileNames.FakerName(entity.Name);
            var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var dbResponseVariableName = $"{lowercaseEntityName}Response";
            var pkName = Entity.PrimaryKeyProperty.Name;
            var lowercaseEntityPk = pkName.LowercaseFirstLetter();
            
            var fakeParent = Utilities.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);

            return $@"[Test]
    public async Task can_delete_{entity.Name.ToLower()}_from_db()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
        await InsertAsync({fakeEntityVariableName});
        var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());
        var {lowercaseEntityPk} = {lowercaseEntityName}.{pkName};

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityPk});
        await SendAsync(command);
        var {dbResponseVariableName} = await ExecuteDbContextAsync(db => db.{entity.Plural}.ToListAsync());

        // Assert
        {dbResponseVariableName}.Count.Should().Be(0);
    }}";
        }

        private static string GetDeleteWithoutKeyTest(string commandName, Entity entity, string featureName)
        {
            var badId = Utilities.GetRandomId(Entity.PrimaryKeyProperty.Type);

            return badId == "" ? "" : $@"

    [Test]
    public async Task delete_{entity.Name.ToLower()}_throws_notfoundexception_when_record_does_not_exist()
    {{
        // Arrange
        var badId = {badId};

        // Act
        var command = new {featureName}.{commandName}(badId);
        Func<Task> act = () => SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }}";
        }

        private static string SoftDeleteTest(string commandName, Entity entity, string featureName)
        {
            var fakeEntity = FileNames.FakerName(entity.Name);
            var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
            var fakeEntityVariableName = $"fake{entity.Name}One";
            var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
            var pkName = Entity.PrimaryKeyProperty.Name;
            var lowercaseEntityPk = pkName.LowercaseFirstLetter();
            
            var fakeParent = Utilities.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);

            return $@"

    [Test]
    public async Task can_softdelete_{entity.Name.ToLower()}_from_db()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
        await InsertAsync({fakeEntityVariableName});
        var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}.SingleOrDefaultAsync());
        var {lowercaseEntityPk} = {lowercaseEntityName}.{pkName};

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityPk});
        await SendAsync(command);
        var deleted{entity.Name} = (await ExecuteDbContextAsync(db => db.{entity.Plural}
            .IgnoreQueryFilters()
            .ToListAsync())
        ).FirstOrDefault();

        // Assert
        deleted{entity.Name}?.IsDeleted.Should().BeTrue();
    }}";
        }
    }
}
