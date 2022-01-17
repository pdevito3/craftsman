namespace Craftsman.Builders.Tests.FunctionalTests
{
    using System;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;

    public class PatchEntityTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string testDirectory, Entity entity, bool isProtected, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(testDirectory, $"Partial{entity.Name}UpdateTests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, testDirectory, classPath, entity, isProtected, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, string testDirectory, ClassPath classPath, Entity entity, bool isProtected, string projectBaseName)
        {
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name);
            var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);
            var permissionsUsing = isProtected 
                ? $"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};"
                : string.Empty;

            var authOnlyTests = isProtected ? $@"
            {EntityTestUnauthorized(entity)}
            {EntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};{permissionsUsing}
using Microsoft.AspNetCore.JsonPatch;
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {PatchEntityTest(entity, isProtected)}{authOnlyTests}
}}";
        }

        private static string PatchEntityTest(Entity entity, bool isProtected)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = Entity.PrimaryKeyProperty.Name;
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            var testName = $"patch_{entity.Name.ToLower()}_returns_nocontent_when_using_valid_patchdoc_on_existing_entity";
            testName += isProtected ? "_and__valid_auth_credentials" : "";
            var clientAuth = isProtected ? @$"

        _client.AddAuth(new[] {{Roles.SuperAdmin}});" : "";

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
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());
        var patchDoc = new JsonPatchDocument<{updateDto}>();
        patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, {lookupVal});{clientAuth}
        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Patch.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.PatchJsonRequestAsync(route, patchDoc);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }}";
        }

        private static string EntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = Entity.PrimaryKeyProperty.Name;
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            return $@"
    [Test]
    public async Task patch_{entity.Name.ToLower()}_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());
        var patchDoc = new JsonPatchDocument<{updateDto}>();
        patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, {lookupVal});

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Patch.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.PatchJsonRequestAsync(route, patchDoc);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
        }

        private static string EntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = Entity.PrimaryKeyProperty.Name;
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
            var lookupVal = $@"""Easily Identified Value For Test""";
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            return $@"
    [Test]
    public async Task patch_{entity.Name.ToLower()}_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());
        var patchDoc = new JsonPatchDocument<{updateDto}>();
        patchDoc.Replace({entity.Lambda} => {entity.Lambda}.{myProp.Name}, {lookupVal});
        _client.AddAuth();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Patch.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.PatchJsonRequestAsync(route, patchDoc);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
        }
    }
}