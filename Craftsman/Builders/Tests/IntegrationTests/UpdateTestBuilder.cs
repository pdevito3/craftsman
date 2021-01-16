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

    public class UpdateTestBuilder
    {
        public static void CreateEntityUpdateTests(string solutionDirectory, Entity entity, string solutionName, string dbContextName)
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
                    var data = UpdateIntegrationTestFileText(classPath, entity, solutionName, dbContextName);
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

        private static string UpdateIntegrationTestFileText(ClassPath classPath, Entity entity, string solutionName, string dbContextName)
        {
            return @$"
namespace {classPath.ClassNamespace}
{{
    using Application.Dtos.{entity.Name};
    using FluentAssertions;
    using {solutionName}.Tests.Fakes.{entity.Name};
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using WebApi;
    using System.Collections.Generic;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.Extensions.DependencyInjection;    
    using Microsoft.AspNetCore.JsonPatch;
    using System.Linq;
    using AutoMapper;
    using Bogus;
    using Application.Mappings;
    using System.Text;
    using Application.Wrappers;

    [Collection(""Sequential"")]
    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : IClassFixture<CustomWebApplicationFactory>
    {{ 
        private readonly CustomWebApplicationFactory _factory;

        public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}(CustomWebApplicationFactory factory)
        {{
            _factory = factory;
        }}

        {UpdateEntityTest(entity, dbContextName)}
        {PutEntityTest(entity, dbContextName)}
    }} 
}}";
        }

        private static string UpdateEntityTest(Entity entity, string dbContextName)
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

            if (myProp == null)
                return "";
            else
                return $@"
        [Fact]
        public async Task Patch{entity.Name}204AndFieldsWereSuccessfullyUpdated()
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
            }});

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

        private static string PutEntityTest(Entity entity, string dbContextName)
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

            if (myProp == null)
                return "";
            else
                return $@"
        [Fact]
        public async Task Put{entity.Name}ReturnsBodyAndFieldsWereSuccessfullyUpdated()
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
            }});

            var serialized{entity.Name}ToUpdate = JsonConvert.SerializeObject(expectedFinalObject);

            // Act
            // get the value i want to update. assumes I can use sieve for this field. if this is not an option, just use something else
            var getResult = await client.GetAsync($""api/{entity.Plural}/?filters={myProp.Name}=={{fake{entity.Name}One.{myProp.Name}}}"")
                .ConfigureAwait(false);
            var getResponseContent = await getResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var getResponse = JsonConvert.DeserializeObject<Response<IEnumerable<{Utilities.GetDtoName(entity.Name, Dto.Read)}>>>(getResponseContent);
            var id = getResponse.Data.FirstOrDefault().{entity.PrimaryKeyProperty.Name};

            // put it
            var patchResult = await client.PutAsJsonAsync($""api/{entity.Plural}/{{id}}"", expectedFinalObject)
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
    }
}
