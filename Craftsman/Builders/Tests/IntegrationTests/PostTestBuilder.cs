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

    public class PostTestBuilder
    {
        public static void CreateEntityWriteTests(string solutionDirectory, ApiTemplate template, Entity entity)
        {
            try
            {
                var classPath = ClassPathHelper.TestEntityIntegrationClassPath(solutionDirectory, $"Create{entity.Name}IntegrationTests.cs", entity.Name, template.SolutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = CreateIntegrationTestFileText(classPath, template, entity);
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

        private static string CreateIntegrationTestFileText(ClassPath classPath, ApiTemplate template, Entity entity)
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
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using WebApi;
    using System.Collections.Generic;

    [Collection(""Sequential"")]
    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : IClassFixture<CustomWebApplicationFactory>
    {{ 
        private readonly CustomWebApplicationFactory _factory;

        public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}(CustomWebApplicationFactory factory)
        {{
            _factory = factory;
        }}

        {CreateEntityTest(template, entity)}
    }} 
}}";
        }

        private static string CreateEntityTest(ApiTemplate template, Entity entity)
        {
            var assertString = "";
            foreach (var prop in entity.Properties.Where(p => p.IsPrimaryKey == false))
            {
                var newLine = prop == entity.Properties.LastOrDefault() ? "" : $"{Environment.NewLine}";
                assertString += @$"            resultDto.{prop.Name}.Should().Be(fake{entity.Name}.{prop.Name});{newLine}";
            }

            return $@"
        [Fact]
        public async Task Post{entity.Name}ReturnsSuccessCodeAndResourceWithAccurateFields()
        {{
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});
            var fake{entity.Name} = new Fake{Utilities.GetDtoName(entity.Name, Dto.Read)}().Generate();

            // Act
            var httpResponse = await client.PostAsJsonAsync(""api/{entity.Plural}"", fake{entity.Name})
                .ConfigureAwait(false);

            // Assert
            httpResponse.EnsureSuccessStatusCode();

            var resultDto = JsonConvert.DeserializeObject<{Utilities.GetDtoName(entity.Name, Dto.Read)}>(await httpResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(false));

            httpResponse.StatusCode.Should().Be(201);
{assertString}
        }}";
        }
    }
}
