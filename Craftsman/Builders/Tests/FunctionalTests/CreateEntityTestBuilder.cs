namespace Craftsman.Builders.Tests.FunctionalTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class CreateEntityTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, List<Policy> policies, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"Create{entity.Name}Tests.cs", entity.Name, projectBaseName);

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

            var restrictedPolicies = Utilities.GetEndpointPolicies(policies, Endpoint.AddRecord, entity.Name);
            var hasRestrictedEndpoints = restrictedPolicies.Count > 0;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {CreateEntityTestUnauthorized(entity)}
            {CreateEntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace}
{{
    using {fakerClassPath.ClassNamespace};
    using {testUtilClassPath.ClassNamespace};
    using FluentAssertions;
    using NUnit.Framework;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
    {{
        {CreateEntityTest(entity, hasRestrictedEndpoints, policies)}{authOnlyTests}
    }}
}}";
        }

        private static string CreateEntityTest(Entity entity, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = entity.PrimaryKeyProperty.Name;

            var testName = $"Create_{entity.Name}_Returns_Created";
            testName += hasRestrictedEndpoints ? "_WithAuth" : "";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.AddRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"

            _client.AddAuth(new[] {scopes});" : "";

            return $@"[Test]
        public async Task {testName}()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();{clientAuth}

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Create;
            var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(201);
        }}";
        }

        private static string CreateEntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = entity.PrimaryKeyProperty.Name;

            return $@"
        [Test]
        public async Task Create_{entity.Name}_Returns_Unauthorized_Without_Valid_Token()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Create;
            var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(401);
        }}";
        }

        private static string CreateEntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = entity.PrimaryKeyProperty.Name;

            return $@"
        [Test]
        public async Task Create_{entity.Name}_Returns_Forbidden_Without_Proper_Scope()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            _client.AddAuth();

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Create;
            var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(403);
        }}";
        }
    }
}
