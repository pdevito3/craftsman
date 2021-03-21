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

    public class HealthCheckTestBuilder
    {
        public static void CreateHealthCheckTests(string solutionDirectory, string solutionName)
        {
            try
            {
                var classPath = ClassPathHelper.TestEntityIntegrationClassPath(solutionDirectory, $"HealthCheckTests.cs", "", solutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = GetHealthCheckTestFileText(classPath);
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

        private static string GetHealthCheckTestFileText(ClassPath classPath)
        {
            return @$"
namespace {classPath.ClassNamespace}
{{
    using FluentAssertions;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(""Sequential"")]
    public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
    {{
        public HealthCheckTests(CustomWebApplicationFactory factory)
        {{
            _factory = factory;
        }}

        private readonly CustomWebApplicationFactory _factory;
        [Fact]
        public async Task HealthCheckReturn200Code()
        {{
            var appFactory = _factory;
            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            var result = await client.GetAsync($""api/health"")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            // Assert
            result.StatusCode.Should().Be(200);
        }}
    }}
}}";
        }
    }
}
