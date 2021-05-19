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

    public class GetListQueryTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(solutionDirectory, $"{entity.Name}ListQueryTests.cs", entity.Name, projectBaseName);

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
            var featureName = Utilities.GetEntityListFeatureClassName(entity.Name);
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();

            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, featureName, entity.Plural, projectBaseName);

            var sortTests = "";
            var filterTests = "";

            foreach (var prop in entity.Properties.Where(e => e.CanSort && e.Type != "Guid").ToList())
            {
                sortTests += GetEntitiesListSortedInAscOrder(entity, prop);
                sortTests += GetEntitiesListSortedInDescOrder(entity, prop);
            }

            foreach (var prop in entity.Properties.Where(e => e.CanFilter).ToList())
                filterTests += GetEntitiesListFiltered(entity, prop);

            return @$"namespace {classPath.ClassNamespace}
{{
    using {dtoClassPath.ClassNamespace};
    using {fakerClassPath.ClassNamespace};
    using FluentAssertions;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using {featuresClassPath.ClassNamespace};
    using static {testFixtureName};

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
    {{
        {GetEntitiesTest(entity)}
        {GetEntitiesWithPageSizeAndNumberTest(entity)}
        {sortTests}
        {filterTests}
    }}
}}";
        }

        private static string GetEntitiesTest(Entity entity)
        {
            var queryName = Utilities.QueryListName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var entityParams = Utilities.GetDtoName(entity.Name, Dto.ReadParamaters);
            var fakeEntityVariableNameOne = $"fake{entity.Name}One";
            var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();

            return @$"
        [Test]
        public async Task {queryName}_Returns_Resource_With_Accurate_Props()
        {{
            // Arrange
            var {fakeEntityVariableNameOne} = new {fakeEntity} {{ }}.Generate();
            var {fakeEntityVariableNameTwo} = new {fakeEntity} {{ }}.Generate();
            var queryParameters = new {entityParams}();

            await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

            // Act
            var query = new {Utilities.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
            var {lowercaseEntityPluralName} = await SendAsync(query);

            // Assert
            {lowercaseEntityPluralName}.Should().HaveCount(2);
        }}";
        }

        private static string GetEntitiesWithPageSizeAndNumberTest(Entity entity)
        {
            var queryName = Utilities.QueryListName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var entityParams = Utilities.GetDtoName(entity.Name, Dto.ReadParamaters);
            var fakeEntityVariableNameOne = $"fake{entity.Name}One";
            var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
            var fakeEntityVariableNameThree = $"fake{entity.Name}Three";
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();

            return $@"
        [Test]
        public async Task {queryName}_Returns_Expected_Page_Size_And_Number()
        {{
            //Arrange
            var {fakeEntityVariableNameOne} = new {fakeEntity} {{ }}.Generate();
            var {fakeEntityVariableNameTwo} = new {fakeEntity} {{ }}.Generate();
            var {fakeEntityVariableNameThree} = new {fakeEntity} {{ }}.Generate();
            var queryParameters = new {entityParams}() {{ PageSize = 1, PageNumber = 2 }};

            await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo}, {fakeEntityVariableNameThree});

            //Act
            var query = new {Utilities.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
            var {lowercaseEntityPluralName} = await SendAsync(query);

            // Assert
            {lowercaseEntityPluralName}.Should().HaveCount(1);
        }}";
        }

        private static string GetEntitiesListSortedInAscOrder(Entity entity, EntityProperty prop)
        {
            var queryName = Utilities.QueryListName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var entityParams = Utilities.GetDtoName(entity.Name, Dto.ReadParamaters);
            var fakeEntityVariableNameOne = $"fake{entity.Name}One";
            var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();

            var alpha = @$"""alpha""";
            var bravo = @$"""bravo""";

            if (prop.Type == "string")
            {
                // leave variables as is
            }
            else if (prop.Type == "Guid")
            {
                alpha = "Guid.NewGuid()";
                bravo = "Guid.NewGuid()";
            }
            else if (prop.Type.Contains("int"))
            {
                alpha = "1";
                bravo = "2";
            }
            else if (prop.Type.Contains("DateTime"))
            {
                alpha = "DateTime.Now.AddDays(1)";
                bravo = "DateTime.Now.AddDays(2)";
            }
            else
            {
                //no tests generated for other types at this time
                return "";
            }

            return $@"
        [Test]
        public async Task {queryName}_Returns_Sorted_{entity.Name}_{prop.Name}_List_In_Asc_Order()
        {{
            //Arrange
            var {fakeEntityVariableNameOne} = new {fakeEntity} {{ }}.Generate();
            var {fakeEntityVariableNameTwo} = new {fakeEntity} {{ }}.Generate();
            fake{entity.Name}One.{prop.Name} = {bravo};
            fake{entity.Name}Two.{prop.Name} = {alpha};
            var queryParameters = new {entityParams}() {{ SortOrder = ""{prop.Name}"" }};

            await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

            //Act
            var query = new {Utilities.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
            var {lowercaseEntityPluralName} = await SendAsync(query);

            // Assert
            {lowercaseEntityPluralName}
                .FirstOrDefault()
                .Should().BeEquivalentTo(fake{entity.Name}Two, options =>
                    options.ExcludingMissingMembers());
            {lowercaseEntityPluralName}
                .Skip(1)
                .FirstOrDefault()
                .Should().BeEquivalentTo(fake{entity.Name}One, options =>
                    options.ExcludingMissingMembers());
        }}{Environment.NewLine}";
        }

        private static string GetEntitiesListSortedInDescOrder(Entity entity, EntityProperty prop)
        {
            var queryName = Utilities.QueryListName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var entityParams = Utilities.GetDtoName(entity.Name, Dto.ReadParamaters);
            var fakeEntityVariableNameOne = $"fake{entity.Name}One";
            var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();

            var alpha = @$"""alpha""";
            var bravo = @$"""bravo""";

            if (prop.Type == "string")
            {
                // leave variables as is
            }
            else if (prop.Type == "Guid")
            {
                alpha = "Guid.NewGuid()";
                bravo = "Guid.NewGuid()";
            }
            else if (prop.Type.Contains("int"))
            {
                alpha = "1";
                bravo = "2";
            }
            else if (prop.Type.Contains("DateTime"))
            {
                alpha = "DateTime.Now.AddDays(1)";
                bravo = "DateTime.Now.AddDays(2)";
            }
            else
            {
                //no tests generated for other types at this time
                return "";
            }

            return $@"
        [Test]
        public async Task {queryName}_Returns_Sorted_{entity.Name}_{prop.Name}_List_In_Desc_Order()
        {{
            //Arrange
            var {fakeEntityVariableNameOne} = new {fakeEntity} {{ }}.Generate();
            var {fakeEntityVariableNameTwo} = new {fakeEntity} {{ }}.Generate();
            fake{entity.Name}One.{prop.Name} = {bravo};
            fake{entity.Name}Two.{prop.Name} = {alpha};
            var queryParameters = new {entityParams}() {{ SortOrder = ""{prop.Name}"" }};

            await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

            //Act
            var query = new {Utilities.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
            var {lowercaseEntityPluralName} = await SendAsync(query);

            // Assert
            {lowercaseEntityPluralName}
                .FirstOrDefault()
                .Should().BeEquivalentTo(fake{entity.Name}Two, options =>
                    options.ExcludingMissingMembers());
            {lowercaseEntityPluralName}
                .Skip(1)
                .FirstOrDefault()
                .Should().BeEquivalentTo(fake{entity.Name}One, options =>
                    options.ExcludingMissingMembers());
        }}{Environment.NewLine}";
        }

        private static string GetEntitiesListFiltered(Entity entity, EntityProperty prop)
        {
            var queryName = Utilities.QueryListName(entity.Name);
            var fakeEntity = Utilities.FakerName(entity.Name);
            var entityParams = Utilities.GetDtoName(entity.Name, Dto.ReadParamaters);
            var fakeEntityVariableNameOne = $"fake{entity.Name}One";
            var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
            var expectedFilterableProperty = @$"fake{entity.Name}Two.{prop.Name}";

            var alpha = @$"""alpha""";
            var bravo = @$"""bravo""";
            var bravoFilterVal = "bravo";

            if (prop.Type == "string")
            {
                // leave variables as is
            }
            else if (prop.Type == "Guid")
            {
                alpha = "Guid.NewGuid()";
                bravo = "Guid.NewGuid()";
            }
            else if (prop.Type.Contains("int"))
            {
                alpha = "1";
                bravo = "2";
                bravoFilterVal = bravo;
            }
            else if (prop.Type.Contains("DateTime"))
            {
                alpha = "DateTime.Now.AddDays(1)";
                bravo = @$"DateTime.Parse(DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy""))"; // filter by date like this because it needs to be an exact match (in this case)
                bravoFilterVal = @$"{{DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy"")}}";
            }
            else if (prop.Type.Contains("bool"))
            {
                alpha = "false";
                bravo = "true";
                bravoFilterVal = bravo;
            }
            else
            {
                //no tests generated for other types at this time
                return "";
            }

            return $@"
        [Test]
        public async Task {queryName}_Filters_{entity.Name}_{prop.Name}()
        {{
            //Arrange
            var {fakeEntityVariableNameOne} = new {fakeEntity} {{ }}.Generate();
            var {fakeEntityVariableNameTwo} = new {fakeEntity} {{ }}.Generate();
            fake{entity.Name}One.{prop.Name} = {alpha};
            fake{entity.Name}Two.{prop.Name} = {bravo};
            var queryParameters = new {entityParams}() {{ Filters = $""{prop.Name} == {{{expectedFilterableProperty}}}"" }};

            await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

            //Act
            var query = new {Utilities.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
            var {lowercaseEntityPluralName} = await SendAsync(query);

            // Assert
            {lowercaseEntityPluralName}.Should().HaveCount(1);
            {lowercaseEntityPluralName}
                .FirstOrDefault()
                .Should().BeEquivalentTo(fake{entity.Name}Two, options =>
                    options.ExcludingMissingMembers());
        }}{Environment.NewLine}";
        }
    }
}