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

    public class GetTestBuilder
    {
        public static void CreateEntityGetTests(string solutionDirectory, ApiTemplate template, Entity entity)
        {
            try
            {
                var classPath = ClassPathHelper.TestEntityIntegrationClassPath(solutionDirectory, $"Get{entity.Name}IntegrationTests.cs", entity.Name, template.SolutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = GetIntegrationTestFileText(classPath, template, entity);
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

        private static string GetIntegrationTestFileText(ClassPath classPath, ApiTemplate template, Entity entity)
        {
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
    using Infrastructure.Persistence.Contexts;
    using Microsoft.Extensions.DependencyInjection;
    using Application.Wrappers;

    [Collection(""Sequential"")]
    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : IClassFixture<CustomWebApplicationFactory>
    {{ 
        private readonly CustomWebApplicationFactory _factory;

        public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}(CustomWebApplicationFactory factory)
        {{
            _factory = factory;
        }}

        {GetEntityTest(template, entity)}
    }} 
}}";
        }

        private static string GetEntityTest(ApiTemplate template, Entity entity)
        {
            return $@"
        [Fact]
        public async Task Get{entity.Plural}_ReturnsSuccessCodeAndResourceWithAccurateFields()
        {{
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {{
                var context = scope.ServiceProvider.GetRequiredService<{template.DbContext.ContextName}>();
                context.Database.EnsureCreated();

                //context.{entity.Plural}.RemoveRange(context.{entity.Plural});
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two);
                context.SaveChanges();
            }}

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            var result = await client.GetAsync(""api/{entity.Plural}"")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<IEnumerable<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>>(responseContent).Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().ContainEquivalentOf(fake{entity.Name}One, options =>
                options.ExcludingMissingMembers());
            response.Should().ContainEquivalentOf(fake{entity.Name}Two, options =>
                options.ExcludingMissingMembers());
        }}";
        }
    }
}
