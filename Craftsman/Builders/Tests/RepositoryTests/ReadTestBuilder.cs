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
                var classPath = ClassPathHelper.ReadRepositoryTestClassPath(solutionDirectory, $"Get{entity.Name}RepositoryTests.cs", entity.Name, template.SolutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = ReadRepositoryTestFileText(classPath.ClassNamespace, template, entity);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
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

        public static string ReadRepositoryTestFileText(string classNamespace, ApiTemplate template, Entity entity)
        {
            return @$"
namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};
    using FluentAssertions;
    using {template.SolutionName}.Tests.Fakes.{entity.Name};
    using Infrastructure.Persistence.Contexts;
    using Infrastructure.Persistence.Repositories;
    using Infrastructure.Shared.Shared;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using Xunit;

    [Collection(""Sequential"")]
    public class Get{entity.Name}RepositoryTests
    {{ 
        {GetEntityTest(template, entity)}
    }} 
}}";
        }

        public static string GetEntityTest(ApiTemplate template, Entity entity)
        {
            var assertString = "";
            foreach(var prop in entity.Properties)
            {
                var newLine = prop == entity.Properties.LastOrDefault() ? "" : $"{Environment.NewLine}";
                assertString += @$"                {entity.Name.LowercaseFirstLetter()}ById.{prop.Name}.Should().Be(fake{entity.Name}.{prop.Name});{newLine}";
            }

            return $@"
        [Fact]
        public void Get{entity.Name}_ParametersMatchExpectedValues()
        {{
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<{template.DbContext.ContextName}>()
                .UseInMemoryDatabase(databaseName: $""{entity.Name}Db{{Guid.NewGuid()}}"")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fake{entity.Name} = new Fake{entity.Name} {{ }}.Generate();

            //Act
            using (var context = new {template.DbContext.ContextName}(dbOptions, new DateTimeService()))
            {{
                context.{entity.Plural}.AddRange(fake{entity.Name});
                context.SaveChanges();

                var service = new {Utilities.GetRepositoryName(entity.Name,false)}(context, new SieveProcessor(sieveOptions));

                //Assert
                var {entity.Name.LowercaseFirstLetter()}ById = service.Get{entity.Name}(fake{entity.Name}.{entity.Name}Id);
                {assertString}
            }}
        }}";
        }
    }
}
