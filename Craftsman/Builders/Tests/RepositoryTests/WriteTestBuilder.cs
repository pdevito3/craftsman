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

    public class WriteTestBuilder
    {
        public static void CreateEntityWriteTests(string solutionDirectory, ApiTemplate template, Entity entity)
        {
            try
            {
                var classPath = ClassPathHelper.TestRepositoryClassPath(solutionDirectory, $"Create{entity.Name}RepositoryTests.cs", entity.Name, template.SolutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = WriteRepositoryTestFileText(classPath, template, entity);
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
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string WriteRepositoryTestFileText(ClassPath classPath, ApiTemplate template, Entity entity)
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
    using Infrastructure.Shared.Services;

    [Collection(""Sequential"")]
    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
    {{ 
        {CreateEntityTest(template, entity)}
    }} 
}}";
        }

        private static string CreateEntityTest(ApiTemplate template, Entity entity)
        {
            var assertString = "";
            foreach (var prop in entity.Properties)
            {
                var newLine = prop == entity.Properties.LastOrDefault() ? "" : $"{Environment.NewLine}";
                assertString += @$"                {entity.Name.LowercaseFirstLetter()}ById.{prop.Name}.Should().Be(fake{entity.Name}.{prop.Name});{newLine}";
            }

            var mockedUserString = TestBuildingHelpers.GetUserServiceString(template);
            var usingString = TestBuildingHelpers.GetUsingString(template);

            return $@"
        [Fact]
        public void Add{entity.Name}_NewRecordAddedWithProperValues()
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

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));

                context.SaveChanges();
            }}

            //Assert
            using (var context = new {template.DbContext.ContextName}(dbOptions))
            {{
                context.{entity.Plural}.Count().Should().Be(1);

                var service = new {Utilities.GetRepositoryName(entity.Name, false)}(context, new SieveProcessor(sieveOptions));
                var {entity.Name.LowercaseFirstLetter()}ById = service.Get{entity.Name}(fake{entity.Name}.{entity.PrimaryKeyProperty.Name});
                {assertString}
            }}
        }}";
        }
    }
}
