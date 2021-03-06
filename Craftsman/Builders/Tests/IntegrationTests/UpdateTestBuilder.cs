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

    public class UpdateTestBuilder
    {
        public static void CreateEntityUpdateTests(string solutionDirectory, Entity entity, string solutionName, string dbContextName, List<Policy> policies, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.TestEntityIntegrationClassPath(solutionDirectory, $"Update{entity.Name}IntegrationTests.cs", entity.Name, solutionName);

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
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var testFakesClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var profileClassPath = ClassPathHelper.ProfileClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            var restrictedRecordUpdatePolicies = Utilities.GetEndpointPolicies(policies, Endpoint.UpdateRecord, entity.Name);
            var hasRestrictedRecordUpdateEndpoints = restrictedRecordUpdatePolicies.Count > 0;
            var authOnlyTests = hasRestrictedRecordUpdateEndpoints ? $@"
            {UpdateRecordEntityTestUnauthorized(entity)}
            {UpdateRecordEntityTestForbidden(entity)}" : "";

            var restrictedPartialUpdatePolicies = Utilities.GetEndpointPolicies(policies, Endpoint.UpdatePartial, entity.Name);
            var hasRestrictedPartialUpdateEndpoints = restrictedPartialUpdatePolicies.Count > 0;
            authOnlyTests += hasRestrictedPartialUpdateEndpoints ? $@"
            {UpdatePartialEntityTestUnauthorized(entity)}
            {UpdatePartialEntityTestForbidden(entity)}" : "";
            var authUsing = hasRestrictedRecordUpdateEndpoints || hasRestrictedPartialUpdateEndpoints ? $@"
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

        {UpdateEntityTest(entity, dbContextName, hasRestrictedPartialUpdateEndpoints, policies)}
        {PutEntityTest(entity, dbContextName, hasRestrictedRecordUpdateEndpoints, policies)}{authOnlyTests}
    }} 
}}";
        }

        private static string UpdateEntityTest(Entity entity, string dbContextName, bool hasRestrictedPartialUpdateEndpoints, List<Policy> policies)
        {
            var myProp = entity.Properties.Where(e => Utilities.PropTypeCleanup(e.Type) == "string" 
                && e.CanFilter 
                && e.IsPrimaryKey == false
                && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";

            // if no string properties, do one with an int
            if (myProp == null)
            {
                myProp = entity.Properties.Where(e => Utilities.PropTypeCleanup(e.Type).Contains("int") 
                    && e.CanFilter
                    && e.IsPrimaryKey == false
                    && e.CanManipulate).FirstOrDefault();
                lookupVal = "999999";
            }
            var testName = hasRestrictedPartialUpdateEndpoints
                ? @$"Patch{entity.Name}204AndFieldsWereSuccessfullyUpdated_WithAuth"
                : @$"Patch{entity.Name}204AndFieldsWereSuccessfullyUpdated";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.UpdatePartial, Endpoint.GetRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedPartialUpdateEndpoints ? @$"

            client.AddAuth(new[] {scopes});" : "";

            if (myProp == null)
                return "";
            else
                return $@"
        [Fact]
        public async Task {testName}()
        {{
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {{
                cfg.AddProfile<{Utilities.GetProfileName(entity.Name)}>();
            }}).CreateMapper();

            var lookupVal = {lookupVal}; // don't know the id at this scope, so need to have another value to lookup
            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            
            var expectedFinalObject = mapper.Map<{Utilities.GetDtoName(entity.Name, Dto.Read)}>(fake{entity.Name}One);
            expectedFinalObject.{myProp.Name} = lookupVal;

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

            var patchDoc = new JsonPatchDocument<{Utilities.GetDtoName(entity.Name, Dto.Update)}>();
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, lookupVal);
            var serialized{entity.Name}ToUpdate = JsonConvert.SerializeObject(patchDoc);

            // Act
            // get the value i want to update. assumes I can use sieve for this field. if this is not an option, just use something else
            var getResult = await client.GetAsync($""api/{entity.Plural}/?filters={myProp.Name}=={{fake{entity.Name}One.{myProp.Name}}}"")
                .ConfigureAwait(false);
            var getResponseContent = await getResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var getResponse = JsonConvert.DeserializeObject<Response<IEnumerable<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>>(getResponseContent);
            var id = getResponse.Data.FirstOrDefault().{entity.PrimaryKeyProperty.Name};

            // patch it
            var method = new HttpMethod(""PATCH"");
            var patchRequest = new HttpRequestMessage(method, $""api/{entity.Plural}/{{id}}"")
            {{
                Content = new StringContent(serialized{entity.Name}ToUpdate,
                    Encoding.Unicode, ""application/json"")
            }};
            var patchResult = await client.SendAsync(patchRequest)
                .ConfigureAwait(false);

            // get it again to confirm updates
            var checkResult = await client.GetAsync($""api/{entity.Plural}/{{id}}"")
                .ConfigureAwait(false);
            var checkResponseContent = await checkResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var checkResponse = JsonConvert.DeserializeObject<Response<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>(checkResponseContent);

            // Assert
            patchResult.StatusCode.Should().Be(204);
            checkResponse.Should().BeEquivalentTo(expectedFinalObject, options =>
                options.ExcludingMissingMembers());
        }}";
        }

        private static string PutEntityTest(Entity entity, string dbContextName, bool hasRestrictedRecordUpdateEndpoints, List<Policy> policies)
        {
            var myProp = entity.Properties.Where(e => Utilities.PropTypeCleanup(e.Type) == "string"
                && e.CanFilter
                && e.IsPrimaryKey == false
                && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";

            // if no string properties, do one with an int
            if (myProp == null)
            {
                myProp = entity.Properties.Where(e => Utilities.PropTypeCleanup(e.Type).Contains("int")
                    && e.CanFilter
                    && e.IsPrimaryKey == false
                    && e.CanManipulate).FirstOrDefault();
                lookupVal = "999999";
            }
            var testName = hasRestrictedRecordUpdateEndpoints
                ? @$"Put{entity.Name}ReturnsBodyAndFieldsWereSuccessfullyUpdated_WithAuth"
                : @$"Put{entity.Name}ReturnsBodyAndFieldsWereSuccessfullyUpdated";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.UpdateRecord, Endpoint.GetRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedRecordUpdateEndpoints ? @$"

            client.AddAuth(new[] {scopes});" : "";

            if (myProp == null)
                return "";
            else
                return $@"
        [Fact]
        public async Task {testName}()
        {{
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {{
                cfg.AddProfile<{Utilities.GetProfileName(entity.Name)}>();
            }}).CreateMapper();

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var expectedFinalObject = mapper.Map<{Utilities.GetDtoName(entity.Name, Dto.Read)}>(fake{entity.Name}One);
            expectedFinalObject.{myProp.Name} = {lookupVal};

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

            var serialized{entity.Name}ToUpdate = JsonConvert.SerializeObject(expectedFinalObject);

            // Act
            // get the value i want to update. assumes I can use sieve for this field. if this is not an option, just use something else
            var getResult = await client.GetAsync($""api/{entity.Plural}/?filters={myProp.Name}=={{fake{entity.Name}One.{myProp.Name}}}"")
                .ConfigureAwait(false);
            var getResponseContent = await getResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var getResponse = JsonConvert.DeserializeObject<Response<IEnumerable<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>>(getResponseContent);
            var id = getResponse?.Data.FirstOrDefault().{entity.PrimaryKeyProperty.Name};

            // put it
            var putResult = await client.PutAsJsonAsync($""api/{entity.Plural}/{{id}}"", expectedFinalObject)
                .ConfigureAwait(false);

            // get it again to confirm updates
            var checkResult = await client.GetAsync($""api/{entity.Plural}/{{id}}"")
                .ConfigureAwait(false);
            var checkResponseContent = await checkResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var checkResponse = JsonConvert.DeserializeObject<Response<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>(checkResponseContent);

            // Assert
            putResult.StatusCode.Should().Be(204);
            checkResponse.Should().BeEquivalentTo(expectedFinalObject, options =>
                options.ExcludingMissingMembers());
        }}";
        }

        private static string UpdateRecordEntityTestUnauthorized(Entity entity)
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
        public async Task UpdateRecord{entity.Plural}_Returns_Unauthorized_Without_Valid_Token()
        {{
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {{
                cfg.AddProfile<{Utilities.GetProfileName(entity.Name)}>();
            }}).CreateMapper();

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var expectedFinalObject = mapper.Map<{Utilities.GetDtoName(entity.Name, Dto.Read)}>(fake{entity.Name}One);
            var id = fake{entity.Name}One.{entity.PrimaryKeyProperty.Name};

            var patchDoc = new JsonPatchDocument<{Utilities.GetDtoName(entity.Name, Dto.Update)}>();
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, """");
            var serialized{entity.Name}ToUpdate = JsonConvert.SerializeObject(patchDoc);

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            // Act
            var putResult = await client.PutAsJsonAsync($""api/{entity.Plural}/{{id}}"", expectedFinalObject)
                .ConfigureAwait(false);

            // Assert
            putResult.StatusCode.Should().Be(401);
        }}";
        }

        private static string UpdateRecordEntityTestForbidden(Entity entity)
        {
            return $@"
        [Fact]
        public async Task UpdateRecord{entity.Name}_Returns_Forbidden_Without_Proper_Scope()
        {{
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {{
                cfg.AddProfile<{Utilities.GetProfileName(entity.Name)}>();
            }}).CreateMapper();

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var expectedFinalObject = mapper.Map<{Utilities.GetDtoName(entity.Name, Dto.Read)}>(fake{entity.Name}One);
            var id = fake{entity.Name}One.{entity.PrimaryKeyProperty.Name};

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            client.AddAuth(new[] {{ """" }});

            // Act
            var putResult = await client.PutAsJsonAsync($""api/{entity.Plural}/{{id}}"", expectedFinalObject)
                .ConfigureAwait(false);

            // Assert
            putResult.StatusCode.Should().Be(403);
        }}";
        }

        private static string UpdatePartialEntityTestUnauthorized(Entity entity)
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
        public async Task UpdatePartial{entity.Plural}_Returns_Unauthorized_Without_Valid_Token()
        {{
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {{
                cfg.AddProfile<{Utilities.GetProfileName(entity.Name)}>();
            }}).CreateMapper();

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var expectedFinalObject = mapper.Map<{Utilities.GetDtoName(entity.Name, Dto.Read)}>(fake{entity.Name}One);
            var id = fake{entity.Name}One.{entity.PrimaryKeyProperty.Name};

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});
            var patchDoc = new JsonPatchDocument<{Utilities.GetDtoName(entity.Name, Dto.Update)}>();
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, """");
            var serialized{entity.Name}ToUpdate = JsonConvert.SerializeObject(patchDoc);

            // Act
            var method = new HttpMethod(""PATCH"");
            var patchRequest = new HttpRequestMessage(method, $""api/{entity.Plural}/{{id}}"")
            {{
                Content = new StringContent(serialized{entity.Name}ToUpdate,
                    Encoding.Unicode, ""application/json"")
            }};
            var patchResult = await client.SendAsync(patchRequest)
                .ConfigureAwait(false);

            // Assert
            patchResult.StatusCode.Should().Be(401);
        }}";
        }

        private static string UpdatePartialEntityTestForbidden(Entity entity)
        {
            return $@"
        [Fact]
        public async Task UpdatePartial{entity.Name}_Returns_Forbidden_Without_Proper_Scope()
        {{
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {{
                cfg.AddProfile<{Utilities.GetProfileName(entity.Name)}>();
            }}).CreateMapper();

            var fake{entity.Name}One = new Fake{entity.Name} {{ }}.Generate();
            var expectedFinalObject = mapper.Map<{Utilities.GetDtoName(entity.Name, Dto.Read)}>(fake{entity.Name}One);
            var id = fake{entity.Name}One.{entity.PrimaryKeyProperty.Name};

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {{
                AllowAutoRedirect = false
            }});

            client.AddAuth(new[] {{ """" }});

            // Act
            var patchResult = await client.PutAsJsonAsync($""api/{entity.Plural}/{{id}}"", expectedFinalObject)
                .ConfigureAwait(false);

            // Assert
            patchResult.StatusCode.Should().Be(403);
        }}";
        }
    }
}
