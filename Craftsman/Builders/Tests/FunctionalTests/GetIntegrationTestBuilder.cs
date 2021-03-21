namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class GetIntegrationTestBuilder
    {
        public static void CreateEntityGetTests(string solutionDirectory, string solutionName, Entity entity, string dbContextName, List<Policy> policies, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.TestEntityIntegrationClassPath(solutionDirectory, $"Get{entity.Name}IntegrationTests.cs", entity.Name, solutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = GetIntegrationTestFileText(classPath, solutionDirectory, solutionName, entity, dbContextName, policies, projectBaseName);
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

        private static string GetIntegrationTestFileText(ClassPath classPath, string solutionDirectory, string solutionName, Entity entity, string dbContextName, List<Policy> policies, string projectBaseName)
        {
            var httpClientExtensionsClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, solutionName, $"HttpClientExtensions.cs");
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(solutionDirectory, "", projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var testFakesClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);

            var restrictedGetListPolicies = Utilities.GetEndpointPolicies(policies, Endpoint.GetList, entity.Name);
            var hasRestrictedGetListEndpoints = restrictedGetListPolicies.Count > 0;
            var authOnlyTests = hasRestrictedGetListEndpoints ? $@"
            {GetEntitiesTestUnauthorized(entity)}
            {GetEntitiesTestForbidden(entity)}" : "";

            var restrictedGetRecordPolicies = Utilities.GetEndpointPolicies(policies, Endpoint.GetRecord, entity.Name);
            var hasRestrictedGetRecordEndpoints = restrictedGetRecordPolicies.Count > 0;
            authOnlyTests += hasRestrictedGetRecordEndpoints ? $@"
            {GetEntityTestUnauthorized(entity)}
            {GetEntityTestForbidden(entity)}" : "";
            var authUsing = hasRestrictedGetRecordEndpoints || hasRestrictedGetListEndpoints ? $@"
    using {httpClientExtensionsClassPath.ClassNamespace};" : "";

            return @$"
namespace {classPath.ClassNamespace}
{{
    using {dtoClassPath.ClassNamespace};
    using {contextClassPath.ClassNamespace};
    using {testFakesClassPath.ClassNamespace};
    using {wrapperClassPath.ClassNamespace};{authUsing}
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;

    [Collection(""Sequential"")]
    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : IClassFixture<CustomWebApplicationFactory>
    {{ 
        private readonly CustomWebApplicationFactory _factory;

        public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}(CustomWebApplicationFactory factory)
        {{
            _factory = factory;
        }}

        {GetEntitiesTest(entity, dbContextName, hasRestrictedGetListEndpoints, policies)}
        {GetEntityTest(entity, dbContextName, hasRestrictedGetRecordEndpoints, policies)}{authOnlyTests}
    }} 
}}";
        }

        private static string GetEntitiesTest(Entity entity, string dbContextName, bool hasRestrictedGetListEndpoints, List<Policy> policies)
        {
            var testName = hasRestrictedGetListEndpoints
                ? @$"Get{entity.Plural}_ReturnsSuccessCodeAndResourceWithAccurateFields_WithAuth" 
                : @$"Get{entity.Plural}_ReturnsSuccessCodeAndResourceWithAccurateFields";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.GetList }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedGetListEndpoints ? @$"

            client.AddAuth(new[] {scopes});" : "";

            return $@"
        [Fact]
        public async Task {testName}()
        {{
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {{
                var context = scope.ServiceProvider.GetRequiredService<{dbContextName}>();
                context.Database.EnsureCreated();

                //context.{entity.Plural}.RemoveRange(context.{entity.Plural});
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two);
                context.SaveChanges();
            }}

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});{clientAuth}

            var result = await client.GetAsync(""api/{entity.Plural}"")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<IEnumerable<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>>(responseContent)?.Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().ContainEquivalentOf(fake{entity.Name}One, options =>
                options.ExcludingMissingMembers());
            response.Should().ContainEquivalentOf(fake{entity.Name}Two, options =>
                options.ExcludingMissingMembers());
        }}";
        }

        private static string GetEntityTest(Entity entity, string dbContextName, bool hasRestrictedGetRecordEndpoints, List<Policy> policies)
        {
            var testName = hasRestrictedGetRecordEndpoints
                ? @$"Get{entity.Name}_ReturnsSuccessCodeAndResourceWithAccurateFields_WithAuth"
                : @$"Get{entity.Name}_ReturnsSuccessCodeAndResourceWithAccurateFields";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.GetRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedGetRecordEndpoints ? @$"

            client.AddAuth(new[] {scopes});" : "";

            return $@"
        [Fact]
        public async Task {testName}()
        {{
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var fake{entity.Name}Two = new Fake{entity.Name} {{ }}.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {{
                var context = scope.ServiceProvider.GetRequiredService<{dbContextName}>();
                context.Database.EnsureCreated();

                //context.{entity.Plural}.RemoveRange(context.{entity.Plural});
                context.{entity.Plural}.AddRange(fake{entity.Name}One, fake{entity.Name}Two);
                context.SaveChanges();
            }}

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});{clientAuth}

            var result = await client.GetAsync($""api/{entity.Plural}/{{fake{entity.Name}One.{entity.PrimaryKeyProperty.Name}}}"")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>(responseContent)?.Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().BeEquivalentTo(fake{entity.Name}One, options =>
                options.ExcludingMissingMembers());
        }}";
        }

        private static string GetEntitiesTestUnauthorized(Entity entity)
        {
            return $@"
        [Fact]
        public async Task Get{entity.Plural}_Returns_Unauthorized_Without_Valid_Token()
        {{
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            var result = await client.GetAsync(""api/{entity.Plural}"")
                .ConfigureAwait(false);

            // Assert
            result.StatusCode.Should().Be(401);
        }}";
        }

        private static string GetEntityTestUnauthorized(Entity entity)
        {
            return $@"
        [Fact]
        public async Task Get{entity.Name}_Returns_Unauthorized_Without_Valid_Token()
        {{
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            var result = await client.GetAsync($""api/{entity.Plural}/{{fake{entity.Name}One.{entity.PrimaryKeyProperty.Name}}}"")
                .ConfigureAwait(false);

            // Assert
            result.StatusCode.Should().Be(401);
        }}";
        }

        private static string GetEntitiesTestForbidden(Entity entity)
        {
            return $@"
        [Fact]
        public async Task Get{entity.Plural}_Returns_Forbidden_Without_Proper_Scope()
        {{
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            client.AddAuth(new[] {{ """" }});

            var result = await client.GetAsync(""api/{entity.Plural}"")
                .ConfigureAwait(false);

            // Assert
            result.StatusCode.Should().Be(403);
        }}";
        }

        private static string GetEntityTestForbidden(Entity entity)
        {
            return $@"
        [Fact]
        public async Task Get{entity.Name}_Returns_Forbidden_Without_Proper_Scope()
        {{
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            client.AddAuth(new[] {{ """" }});

            var result = await client.GetAsync($""api/{entity.Plural}/{{fake{entity.Name}One.{entity.PrimaryKeyProperty.Name}}}"")
                .ConfigureAwait(false);

            // Assert
            result.StatusCode.Should().Be(403);
        }}";
        }
    }
}
