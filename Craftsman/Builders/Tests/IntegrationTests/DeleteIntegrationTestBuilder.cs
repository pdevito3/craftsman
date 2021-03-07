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

    public class DeleteIntegrationTestBuilder
    {
        public static void CreateEntityDeleteTests(string solutionDirectory, Entity entity, string solutionName, string dbContextName, List<Policy> policies, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.TestEntityIntegrationClassPath(solutionDirectory, $"Delete{entity.Name}IntegrationTests.cs", entity.Name, solutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = UpdateIntegrationTestFileText(classPath, entity, solutionDirectory, solutionName, dbContextName, policies, projectBaseName);
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

        private static string UpdateIntegrationTestFileText(ClassPath classPath, Entity entity, string solutionDirectory, string solutionName, string dbContextName, List<Policy> policies, string projectBaseName)
        {
            var httpClientExtensionsClassPath = ClassPathHelper.HttpClientExtensionsClassPath(solutionDirectory, solutionName, $"HttpClientExtensions.cs");
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(solutionDirectory, "", projectBaseName);
            var profileClassPath = ClassPathHelper.ProfileClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var testFakesClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            var restrictedPolicies = Utilities.GetEndpointPolicies(policies, Endpoint.DeleteRecord, entity.Name);
            var hasRestrictedEndpoints = restrictedPolicies.Count > 0;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {DeleteRecordEntityTestUnauthorized(entity)}
            {DeleteRecordEntityTestForbidden(entity)}" : "";
            var authUsing = hasRestrictedEndpoints ? $@"
    using {httpClientExtensionsClassPath.ClassNamespace};" : "";

            return @$"
namespace {classPath.ClassNamespace}
{{
    using {dtoClassPath.ClassNamespace};
    using FluentAssertions;
    using {testFakesClassPath.ClassNamespace};
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Collections.Generic;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.Extensions.DependencyInjection;    
    using Microsoft.AspNetCore.JsonPatch;
    using System.Linq;
    using AutoMapper;
    using Bogus;
    using {profileClassPath.ClassNamespace};
    using System.Text;
    using {wrapperClassPath.ClassNamespace};{authUsing}

    [Collection(""Sequential"")]
    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : IClassFixture<CustomWebApplicationFactory>
    {{ 
        private readonly CustomWebApplicationFactory _factory;

        public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}(CustomWebApplicationFactory factory)
        {{
            _factory = factory;
        }}

        {DeleteRecordTest(entity, dbContextName, hasRestrictedEndpoints, policies)}{authOnlyTests}
    }} 
}}";
        }

        private static string DeleteRecordTest(Entity entity, string dbContextName, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var myProp = entity.Properties.Where(e => Utilities.PropTypeCleanup(e.Type) == "string"
                && e.CanFilter
                && e.IsPrimaryKey == false
                && e.CanManipulate).FirstOrDefault();

            // if no string properties, do one with an int
            if (myProp == null)
            {
                myProp = entity.Properties.Where(e => Utilities.PropTypeCleanup(e.Type).Contains("int")
                    && e.CanFilter
                    && e.IsPrimaryKey == false
                    && e.CanManipulate).FirstOrDefault();
            }
            var testName = hasRestrictedEndpoints
                ? @$"Delete{entity.Name}204AndFieldsWereSuccessfullyUpdated_WithAuth"
                : @$"Delete{entity.Name}204AndFieldsWereSuccessfullyUpdated";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.DeleteRecord, Endpoint.GetRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"

            client.AddAuth(new[] {scopes});" : "";

            if (myProp == null)
                return "";
            else
                return $@"
        [Fact]
        public async Task {testName}()
        {{
            //Arrange
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {{
                var context = scope.ServiceProvider.GetRequiredService<{dbContextName}> ();
                context.Database.EnsureCreated();

                context.{entity.Plural}.RemoveRange(context.{entity.Plural});
                context.{entity.Plural}.AddRange(fake{entity.Name}One);
                context.SaveChanges();
            }}

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});{clientAuth}

            // Act
            // get the value i want to update. assumes I can use sieve for this field. if this is not an option, just use something else
            var getResult = await client.GetAsync($""api/{entity.Plural}/?filters={myProp.Name}=={{fake{entity.Name}One.{myProp.Name}}}"")
                .ConfigureAwait(false);
            var getResponseContent = await getResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var getResponse = JsonConvert.DeserializeObject<Response<IEnumerable<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>>(getResponseContent);
            var id = getResponse.Data.FirstOrDefault().{entity.PrimaryKeyProperty.Name};

            // delete it
            var method = new HttpMethod(""DELETE"");
            var deleteRequest = new HttpRequestMessage(method, $""api/{entity.Plural}/{{id}}"");
            var deleteResult = await client.SendAsync(deleteRequest)
                .ConfigureAwait(false);

            // get it again to confirm updates
            var checkResult = await client.GetAsync($""api/{entity.Plural}/{{id}}"")
                .ConfigureAwait(false);
            var checkResponseContent = await checkResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var checkResponse = JsonConvert.DeserializeObject<Response<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>(checkResponseContent);

            // Assert
            deleteResult.StatusCode.Should().Be(204);
            checkResponse.Data.Should().Be(null);
        }}";
        }

        private static string DeleteRecordEntityTestUnauthorized(Entity entity)
        {
            var myProp = entity.Properties.Where(e => Utilities.PropTypeCleanup(e.Type) == "string"
                && e.CanFilter
                && e.IsPrimaryKey == false
                && e.CanManipulate).FirstOrDefault();

            if (myProp == null)
                return "";
            else
                return $@"
        [Fact]
        public async Task DeleteRecord{entity.Plural}_Returns_Unauthorized_Without_Valid_Token()
        {{
            //Arrange
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var id = fake{entity.Name}One.{entity.PrimaryKeyProperty.Name};

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            // Act
            var method = new HttpMethod(""DELETE"");
            var deleteRequest = new HttpRequestMessage(method, $""api/{entity.Plural}/{{id}}"");
            var deleteResult = await client.SendAsync(deleteRequest)
                .ConfigureAwait(false);

            // Assert
            deleteResult.StatusCode.Should().Be(401);
        }}";
        }

        private static string DeleteRecordEntityTestForbidden(Entity entity)
        {
            return $@"
        [Fact]
        public async Task DeleteRecord{entity.Name}_Returns_Forbidden_Without_Proper_Scope()
        {{
            //Arrange
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var id = fake{entity.Name}One.{entity.PrimaryKeyProperty.Name};

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            client.AddAuth(new[] {{ """" }});

            // Act
            var method = new HttpMethod(""DELETE"");
            var deleteRequest = new HttpRequestMessage(method, $""api/{entity.Plural}/{{id}}"");
            var deleteResult = await client.SendAsync(deleteRequest)
                .ConfigureAwait(false);

            // Assert
            deleteResult.StatusCode.Should().Be(403);
        }}";
        }
    }
}
