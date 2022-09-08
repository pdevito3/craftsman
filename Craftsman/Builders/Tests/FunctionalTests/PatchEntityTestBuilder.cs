﻿namespace Craftsman.Builders.Tests.FunctionalTests;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class PatchEntityTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public PatchEntityTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string srcDirectory, string testDirectory, Entity entity, bool isProtected, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestClassPath(testDirectory, $"Partial{entity.Name}UpdateTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, srcDirectory, testDirectory, classPath, entity, isProtected, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string srcDirectory, string testDirectory, ClassPath classPath, Entity entity, bool isProtected, string projectBaseName)
    {
        var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");

        var permissionsUsing = isProtected
            ? $"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};{Environment.NewLine}using {rolesClassPath.ClassNamespace};"
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
using Bogus;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    private readonly Faker _faker = new Faker();

    {PatchEntityTest(entity, isProtected)}{authOnlyTests}
}}";
    }

    private static string PatchEntityTest(Entity entity, bool isProtected)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);
        var myProp = entity.Properties.Where(e => e.Type == "string" && e.CanManipulate).FirstOrDefault();
        var lookupVal = "_faker.Lorem.Word()";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var testName = $"patch_{entity.Name.ToLower()}_returns_nocontent_when_using_valid_patchdoc_on_existing_entity";
        testName += isProtected ? "_and__valid_auth_credentials" : "";
        var clientAuth = isProtected ? @$"

        var user = await AddNewSuperAdmin();
        _client.AddAuth(user.Identifier);" : "";

        // if no string properties, do one with an int
        if (myProp == null)
        {
            myProp = entity.Properties.FirstOrDefault(e => e.Type.Contains("int") && e.CanManipulate);
            lookupVal = "_faker.Random.Int()";
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
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);
        var myProp = entity.Properties.FirstOrDefault(e => e.Type == "string" && e.CanManipulate);
        var lookupVal = "_faker.Lorem.Word()";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

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
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);
        var myProp = entity.Properties.FirstOrDefault(e => e.Type == "string" && e.CanManipulate);
        var lookupVal = "_faker.Lorem.Word()";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

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
