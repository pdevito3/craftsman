namespace Craftsman.Builders.Tests.FunctionalTests
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

    public class PatchEntityTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, List<Policy> policies, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"Partial{entity.Name}UpdateTests.cs", entity.Name, projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = WriteTestFileText(solutionDirectory, classPath, entity, policies, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, List<Policy> policies, string projectBaseName)
        {
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            var restrictedPolicies = Utilities.GetEndpointPolicies(policies, Endpoint.UpdatePartial, entity.Name);
            var hasRestrictedEndpoints = restrictedPolicies.Count > 0;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {EntityTestUnauthorized(entity)}
            {EntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace}
{{
    using {fakerClassPath.ClassNamespace};
    using {dtoClassPath.ClassNamespace};
    using {testUtilClassPath.ClassNamespace};
    using Microsoft.AspNetCore.JsonPatch;
    using FluentAssertions;
    using NUnit.Framework;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
    {{
        {PatchEntityTest(entity, hasRestrictedEndpoints, policies)}{authOnlyTests}
    }}
}}";
        }

        private static string PatchEntityTest(Entity entity, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = entity.PrimaryKeyProperty.Name;
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";

            var testName = $"Patch_{entity.Name}_Returns_NoContent";
            testName += hasRestrictedEndpoints ? "_WithAuth" : "";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.UpdatePartial }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"

            _client.AddAuth(new[] {scopes});" : "";


            // if no string properties, do one with an int
            if (myProp == null)
            {
                myProp = entity.Properties.Where(e => e.Type.Contains("int") && e.CanManipulate).FirstOrDefault();
                lookupVal = "999999";
            }

            if (myProp == null)
                return "// no patch tests were created";

            return $@"[Test]
        public async Task {testName}()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            var patchDoc = new JsonPatchDocument<{updateDto}>();
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, {lookupVal});{clientAuth}

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Patch.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
            var result = await _client.PatchJsonRequestAsync(route, patchDoc);

            // Assert
            result.StatusCode.Should().Be(204);
        }}";
        }

        private static string EntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = entity.PrimaryKeyProperty.Name;
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";

            return $@"
        [Test]
        public async Task Patch_{entity.Name}_Returns_Unauthorized_Without_Valid_Token()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            var patchDoc = new JsonPatchDocument<{updateDto}>();
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, {lookupVal});

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Patch.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
            var result = await _client.PatchJsonRequestAsync(route, patchDoc);

            // Assert
            result.StatusCode.Should().Be(401);
        }}";
        }

        private static string EntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = entity.PrimaryKeyProperty.Name;
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";

            return $@"
        [Test]
        public async Task Patch_{entity.Name}_Returns_Forbidden_Without_Proper_Scope()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            var patchDoc = new JsonPatchDocument<{updateDto}>();
            patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, {lookupVal});
            _client.AddAuth();

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Patch.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
            var result = await _client.PatchJsonRequestAsync(route, patchDoc);

            // Assert
            result.StatusCode.Should().Be(403);
        }}";
        }
    }
}
