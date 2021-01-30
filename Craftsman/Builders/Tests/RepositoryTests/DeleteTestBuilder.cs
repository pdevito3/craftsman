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

    public class DeleteTestBuilder
    {
        public static void DeleteEntityWriteTests(string solutionDirectory, Entity entity, string solutionName, string dbContextName)
        {
            try
            {
                var classPath = ClassPathHelper.TestRepositoryClassPath(solutionDirectory, $"Delete{entity.Name}RepositoryTests.cs", entity.Name, solutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = DeleteRepositoryTestFileText(classPath, entity, solutionName, dbContextName);
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

        private static string DeleteRepositoryTestFileText(ClassPath classPath, Entity entity, string solutionName, string dbContextName)
        {
            var assertString = "";
            foreach (var prop in entity.Properties)
            {
                var newLine = prop == entity.Properties.LastOrDefault() ? "" : $"{Environment.NewLine}";
                assertString += @$"                {entity.Name.LowercaseFirstLetter()}ById.{prop.Name}.Should().Be(fake{entity.Name}.{prop.Name});{newLine}";
            }

            return @$"
namespace {classPath.ClassNamespace}
{{
    using Application.Dtos.{entity.Name};
    using FluentAssertions;
    using {solutionName}.Tests.Fakes.{entity.Name};
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
        {DeleteEntityTest(entity, dbContextName)}
    }} 
}}";
        }

        private static string DeleteEntityTest(Entity entity, string dbContextName)
        {
            var usingString = TestBuildingHelpers.GetUsingString(dbContextName);

            return $@"
        [Fact]
        public void Delete{entity.Name}_ReturnsProperCount()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{dbContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Three = new Fake{entity.Name} {{ }}.Generate();

            //Act
            {usingString}
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two, fake{entity.Name}Three);

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));
                service.Delete{entity.Name}(fake{entity.Name}Two);

                context.SaveChanges();

                //Assert
                var {entity.Name.LowercaseFirstLetter()}List = context.{entity.Plural}.ToList();

                {entity.Name.LowercaseFirstLetter()}List.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                {entity.Name.LowercaseFirstLetter()}List.Should().ContainEquivalentOf(fake{entity.Name}One);
                {entity.Name.LowercaseFirstLetter()}List.Should().ContainEquivalentOf(fake{entity.Name}Three);
                Assert.DoesNotContain({entity.Name.LowercaseFirstLetter()}List, {entity.Lambda} => {entity.Lambda} == fake{entity.Name}Two);

                context.Database.EnsureDeleted();
            }}
        }}";
        }
    }
}
