namespace Craftsman.Builders.Tests.RepositoryTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ReadTestBuilder
    {
        public static void CreateEntityReadTests(string solutionDirectory, ApiTemplate template, Entity entity)
        {
            try
            {
                var classPath = ClassPathHelper.TestRepositoryClassPath(solutionDirectory, $"Get{entity.Name}RepositoryTests.cs", entity.Name, template.SolutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = ReadRepositoryTestFileText(classPath, template, entity);
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

        private static string ReadRepositoryTestFileText(ClassPath classPath, ApiTemplate template, Entity entity)
        {
            var sortTests = "";

            foreach(var prop in entity.Properties.Where(e => e.CanSort && e.Type != "Guid").ToList())
            {
                sortTests += GetEntitiesListSortedInAscOrder(template, entity, prop);
                sortTests += GetEntitiesListSortedInDescOrder(template, entity, prop);
            }

            return @$"
namespace {classPath.ClassNamespace}
{{
    using Application.Dtos.{entity.Name};
    using FluentAssertions;
    using {template.SolutionName}.Tests.Fakes.{entity.Name};
    using Infrastructure.Persistence.Contexts;
    using Infrastructure.Persistence.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using Xunit;
    using Application.Interfaces;
    using Moq;

    [Collection(""Sequential"")]
    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
    {{ 
        {GetEntityTest(template, entity)}
        {GetEntitiesTest(template, entity)}
        {GetEntitiesWithPageSizeTest(template,entity)}
        {GetEntitiesWithPageSizeandNumberTest(template, entity)}
        {sortTests}
        {GetEntitiesListCanFilterTests(template,entity)}
    }} 
}}";
        }

        private static string GetEntityTest(ApiTemplate template, Entity entity)
        {
            var assertString = "";

            foreach(var prop in entity.Properties)
            {
                var newLine = prop == entity.Properties.LastOrDefault() ? "" : $"{Environment.NewLine}";
                assertString += @$"                {entity.Name.LowercaseFirstLetter()}ById.{prop.Name}.Should().Be(fake{entity.Name}.{prop.Name});{newLine}";
            }

            var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
            var usingString = TestBuildingHelpers.GetUsingString(template);

            return $@"
        [Fact]
        public void Get{entity.Name}_ParametersMatchExpectedValues()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());{mockedUserString}

            var fake{entity.Name} = new Fake{entity.Name} {{ }}.Generate();

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name});
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name,false)}(context, new SieveProcessor(sieveOptions));

                //Assert
                var {entity.Name.LowercaseFirstLetter()}ById = service.Get{entity.Name}(fake{entity.Name}.{entity.PrimaryKeyProperty.Name});
                {assertString}
            }}
        }}";
        }

        private static string GetEntitiesTest(ApiTemplate template, Entity entity)
        {
            var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
            var usingString = TestBuildingHelpers.GetUsingString(template);
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);

            return $@"
        [Fact]
        public async void {getListMethodName}_CountMatchesAndContainsEquivalentObjects()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());{mockedUserString}

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Three = new Fake{entity.Name} {{ }}.Generate();

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two, fake{entity.Name}Three);
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));

                var {entity.Name.LowercaseFirstLetter()}Repo = await service.{getListMethodName}(new {Utilities.GetDtoName(entity.Name,Enums.Dto.ReadParamaters)}());

                //Assert
                {entity.Name.LowercaseFirstLetter()}Repo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(3);

                {entity.Name.LowercaseFirstLetter()}Repo.Should().ContainEquivalentOf(fake{entity.Name}One);
                {entity.Name.LowercaseFirstLetter()}Repo.Should().ContainEquivalentOf(fake{entity.Name}Two);
                {entity.Name.LowercaseFirstLetter()}Repo.Should().ContainEquivalentOf(fake{entity.Name}Three);

                context.Database.EnsureDeleted();
            }}
        }}";
        }

        private static string GetEntitiesWithPageSizeTest(ApiTemplate template, Entity entity)
        {
            var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
            var usingString = TestBuildingHelpers.GetUsingString(template);
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);
            var alpha = "";
            var bravo = "";
            var charlie = "";

            if (entity.PrimaryKeyProperty.Type == "string")
            {
                alpha = @$"""alpha""";
                bravo = @$"""bravo""";
                charlie = @$"""charlie""";
            }
            else if (entity.PrimaryKeyProperty.Type == "Guid")
            {
                alpha = @$"Guid.Parse(""547ee3d9-5241-4ce3-93f6-a65700bd36ca"")";
                bravo = @$"Guid.Parse(""621fab6d-2487-43f4-aec2-354fa54089da"")";
                charlie = @$"Guid.Parse(""f9335b96-63dd-412e-935b-102463b9f245"")";
            }
            else if (entity.PrimaryKeyProperty.Type.Contains("int"))
            {
                alpha = "1";
                bravo = "2";
                charlie = "3";
            }
            else if (entity.PrimaryKeyProperty.Type.Contains("DateTime"))
            {
                alpha = "DateTime.Now.AddDays(1)";
                bravo = "DateTime.Now.AddDays(2)";
                charlie = "DateTime.Now.AddDays(3)";
            }
            else
            {
                //no tests generated for other types at this time
                return "";
            }

            return $@"
        [Fact]
        public async void {getListMethodName}_ReturnExpectedPageSize()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());{mockedUserString}

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Three = new Fake{entity.Name} {{ }}.Generate();
            
            // need id's due to default sorting
            fake{entity.Name}One.{entity.PrimaryKeyProperty.Name} = {alpha};
            fake{entity.Name}Two.{entity.PrimaryKeyProperty.Name} = {bravo};
            fake{entity.Name}Three.{entity.PrimaryKeyProperty.Name} = {charlie};

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two, fake{entity.Name}Three);
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));

                var {entity.Name.LowercaseFirstLetter()}Repo = await service.{getListMethodName}(new {Utilities.GetDtoName(entity.Name, Enums.Dto.ReadParamaters)} {{ PageSize = 2 }});

                //Assert
                {entity.Name.LowercaseFirstLetter()}Repo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                {entity.Name.LowercaseFirstLetter()}Repo.Should().ContainEquivalentOf(fake{entity.Name}One);
                {entity.Name.LowercaseFirstLetter()}Repo.Should().ContainEquivalentOf(fake{entity.Name}Two);

                context.Database.EnsureDeleted();
            }}
        }}";
        }

        private static string GetEntitiesWithPageSizeandNumberTest(ApiTemplate template, Entity entity)
        {
            var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
            var usingString = TestBuildingHelpers.GetUsingString(template);
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);
            var alpha = "";
            var bravo = "";
            var charlie = "";

            if (entity.PrimaryKeyProperty.Type == "string")
            {
                alpha = @$"""alpha""";
                bravo = @$"""bravo""";
                charlie = @$"""charlie""";
            }
            else if (entity.PrimaryKeyProperty.Type == "Guid")
            {
                alpha = @$"Guid.Parse(""547ee3d9-5241-4ce3-93f6-a65700bd36ca"")";
                bravo = @$"Guid.Parse(""621fab6d-2487-43f4-aec2-354fa54089da"")";
                charlie = @$"Guid.Parse(""f9335b96-63dd-412e-935b-102463b9f245"")";
            }
            else if (entity.PrimaryKeyProperty.Type.Contains("int"))
            {
                alpha = "1";
                bravo = "2";
                charlie = "3";
            }
            else if (entity.PrimaryKeyProperty.Type.Contains("DateTime"))
            {
                alpha = "DateTime.Now.AddDays(1)";
                bravo = "DateTime.Now.AddDays(2)";
                charlie = "DateTime.Now.AddDays(3)";
            }
            else
            {
                //no tests generated for other types at this time
                return "";
            }

            return $@"
        [Fact]
        public async void {getListMethodName}_ReturnExpectedPageNumberAndSize()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());{mockedUserString}

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Three = new Fake{entity.Name} {{ }}.Generate();
            
            // need id's due to default sorting
            fake{entity.Name}One.{entity.PrimaryKeyProperty.Name} = {alpha};
            fake{entity.Name}Two.{entity.PrimaryKeyProperty.Name} = {bravo};
            fake{entity.Name}Three.{entity.PrimaryKeyProperty.Name} = {charlie};

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two, fake{entity.Name}Three);
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));

                var {entity.Name.LowercaseFirstLetter()}Repo = await service.{getListMethodName}(new {Utilities.GetDtoName(entity.Name, Enums.Dto.ReadParamaters)} {{ PageSize = 1, PageNumber = 2 }});

                //Assert
                {entity.Name.LowercaseFirstLetter()}Repo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(1);

                {entity.Name.LowercaseFirstLetter()}Repo.Should().ContainEquivalentOf(fake{entity.Name}Two);

                context.Database.EnsureDeleted();
            }}
        }}";
        }
       
        private static string GetEntitiesListSortedInAscOrder(ApiTemplate template, Entity entity, EntityProperty prop)
        {
            var alpha = "";
            var bravo = "";
            var charlie = "";

            if (prop.Type == "string")
            {
                alpha = @$"""alpha""";
                bravo = @$"""bravo""";
                charlie = @$"""charlie""";
            }
            else if (prop.Type == "Guid")
            {
                alpha = "Guid.NewGuid()";
                bravo = "Guid.NewGuid()";
                charlie = "Guid.NewGuid()";
            }
            else if (prop.Type.Contains("int"))
            {
                alpha = "1";
                bravo = "2";
                charlie = "3";
            }
            else if (prop.Type.Contains("DateTime"))
            {
                alpha = "DateTime.Now.AddDays(1)";
                bravo = "DateTime.Now.AddDays(2)";
                charlie = "DateTime.Now.AddDays(3)";
            }
            else
            {
                //no tests generated for other types at this time
                return "";
            }

            var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
            var usingString = TestBuildingHelpers.GetUsingString(template);
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);

            return $@"
        [Fact]
        public async void {getListMethodName}_List{prop.Name}SortedInAscOrder()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());{mockedUserString}

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}One.{prop.Name} = {bravo};

            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}Two.{prop.Name} = {alpha};

            var fake{entity.Name}Three = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}Three.{prop.Name} = {charlie};

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two, fake{entity.Name}Three);
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));

                var {entity.Name.LowercaseFirstLetter()}Repo = await service.{getListMethodName}(new {Utilities.GetDtoName(entity.Name, Enums.Dto.ReadParamaters)} {{ SortOrder = ""{prop.Name}"" }});

                //Assert
                {entity.Name.LowercaseFirstLetter()}Repo.Should()
                    .ContainInOrder(fake{entity.Name}Two, fake{entity.Name}One, fake{entity.Name}Three);

                context.Database.EnsureDeleted();
            }}
        }}{Environment.NewLine}";
        }

        private static string GetEntitiesListSortedInDescOrder(ApiTemplate template, Entity entity, EntityProperty prop)
        {
            var alpha = "";
            var bravo = "";
            var charlie = "";

            if (prop.Type == "string")
            {
                alpha = @$"""alpha""";
                bravo = @$"""bravo""";
                charlie = @$"""charlie""";
            }
            else if (prop.Type == "Guid")
            {
                alpha = "Guid.NewGuid()";
                bravo = "Guid.NewGuid()";
                charlie = "Guid.NewGuid()";
            }
            else if (prop.Type.Contains("int"))
            {
                alpha = "1";
                bravo = "2";
                charlie = "3";
            }
            else if (prop.Type.Contains("DateTime"))
            {
                alpha = "DateTime.Now.AddDays(1)";
                bravo = "DateTime.Now.AddDays(2)";
                charlie = "DateTime.Now.AddDays(3)";
            }
            else
            {
                //no tests generated for other types at this time
                return "";
            }

            var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
            var usingString = TestBuildingHelpers.GetUsingString(template);
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);

            return $@"
        [Fact]
        public async void {getListMethodName}_List{prop.Name}SortedInDescOrder()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());{mockedUserString}

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}One.{prop.Name} = {bravo};

            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}Two.{prop.Name} = {alpha};

            var fake{entity.Name}Three = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}Three.{prop.Name} = {charlie};

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two, fake{entity.Name}Three);
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));

                var {entity.Name.LowercaseFirstLetter()}Repo = await service.{getListMethodName}(new {Utilities.GetDtoName(entity.Name, Enums.Dto.ReadParamaters)} {{ SortOrder = ""-{prop.Name}"" }});

                //Assert
                {entity.Name.LowercaseFirstLetter()}Repo.Should()
                    .ContainInOrder(fake{entity.Name}Three, fake{entity.Name}One, fake{entity.Name}Two);

                context.Database.EnsureDeleted();
            }}
        }}{Environment.NewLine}";
        }

        private static string GetEntitiesListCanFilterTests(ApiTemplate template, Entity entity)
        {
            var filterTests = "";
            foreach (var prop in entity.Properties.Where(e => e.CanFilter).ToList())
            {
                var alpha = "";
                var bravo = "";
                var charlie = "";
                var alphaFilterVal = "";
                var bravoFilterVal = "";
                var charlieFilterVal = "";

                if (prop.Type == "string")
                {
                    alpha = @$"""alpha""";
                    bravo = @$"""bravo""";
                    charlie = @$"""charlie""";
                    alphaFilterVal = "alpha";
                    bravoFilterVal = "bravo";
                    charlieFilterVal = "charlie";
                }
                else if (prop.Type == "Guid")
                {
                    alpha = "Guid.NewGuid()";
                    alphaFilterVal = "alpha";
                    bravo = "Guid.NewGuid()";
                    bravoFilterVal = "bravo";
                    charlie = "Guid.NewGuid()";
                    charlieFilterVal = "charlie";
                }
                else if (prop.Type.Contains("int"))
                {
                    alpha = "1";
                    alphaFilterVal = alpha;
                    bravo = "2";
                    bravoFilterVal = bravo;
                    charlie = "3";
                    charlieFilterVal = charlie;
                }
                else if (prop.Type.Contains("DateTime"))
                {
                    alpha = @$"DateTime.Now.AddDays(1)";
                    bravo = @$"DateTime.Parse(DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy""))"; // fitler by date like this because it needs to be an exact match (in this case)
                    bravoFilterVal = @$"{{DateTime.Now.AddDays(2).ToString(""MM/dd/yyyy"")}}";
                    charlie = @$"DateTime.Now.AddDays(3)";
                }
                else if (prop.Type.Contains("bool"))
                {
                    alpha = "false";
                    alphaFilterVal = alpha;
                    bravo = "true";
                    bravoFilterVal = bravo;
                    charlie = "false";
                    charlieFilterVal = charlie;
                }
                else
                {
                    //no tests generated for other types at this time
                    return "";
                }

                var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
                var usingString = TestBuildingHelpers.GetUsingString(template);
                var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);
                var expectedFilterableProperty = @$"fake{entity.Name}Two.{prop.Name}";

                filterTests += $@"
        [Fact]
        public async void {getListMethodName}_Filter{prop.Name}ListWithExact()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());{mockedUserString}

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}One.{prop.Name} = {alpha};

            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();
            {expectedFilterableProperty} = {bravo};

            var fake{entity.Name}Three = new Fake{entity.Name} {{ }}.Generate();
            fake{entity.Name}Three.{prop.Name} = {charlie};

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two, fake{entity.Name}Three);
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));

                var {entity.Name.LowercaseFirstLetter()}Repo = await service.{getListMethodName}(new {Utilities.GetDtoName(entity.Name, Enums.Dto.ReadParamaters)} {{ Filters = $""{prop.Name} == {{{expectedFilterableProperty}}}"" }});

                //Assert
                {entity.Name.LowercaseFirstLetter()}Repo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }}
        }}{Environment.NewLine}";
            }

            return filterTests;
        }
    }
}
