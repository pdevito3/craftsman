﻿namespace Craftsman.Builders.Tests.FunctionalTests;

using System;
using System.IO;
using Domain;
using Helpers;
using Services;

public class GetEntityListTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetEntityListTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, Entity entity, bool isProtected, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestClassPath(testDirectory, $"Get{entity.Name}ListTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, classPath, entity, isProtected, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, ClassPath classPath, Entity entity, bool isProtected, string projectBaseName)
    {
        var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");

        var permissionsUsing = isProtected
            ? $"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};{Environment.NewLine}using {rolesClassPath.ClassNamespace};"
            : string.Empty;

        var authOnlyTests = isProtected ? $@"
            {GetEntityTestUnauthorized(entity)}
            {GetEntityTestForbidden(entity)}" : "";

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};{permissionsUsing}
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {GetEntityTest(entity, isProtected)}{authOnlyTests}
}}";
    }

    private static string GetEntityTest(Entity entity, bool isProtected)
    {
        var testName = $"get_{entity.Name.ToLower()}_list_returns_success";
        testName += isProtected ? "_using_valid_auth_credentials" : "";
        var clientAuth = isProtected ? @$"

        var user = await AddNewSuperAdmin();
        _client.AddAuth(user.Identifier);" : "";

        return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        {clientAuth ?? "// N/A"}

        // Act
        var result = await _client.GetRequestAsync(ApiRoutes.{entity.Plural}.GetList);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }}";
    }

    private static string GetEntityTestUnauthorized(Entity entity)
    {
        return $@"
    [Test]
    public async Task get_{entity.Name.ToLower()}_list_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        // N/A

        // Act
        var result = await _client.GetRequestAsync(ApiRoutes.{entity.Plural}.GetList);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
    }

    private static string GetEntityTestForbidden(Entity entity)
    {
        return $@"
    [Test]
    public async Task get_{entity.Name.ToLower()}_list_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        _client.AddAuth();

        // Act
        var result = await _client.GetRequestAsync(ApiRoutes.{entity.Plural}.GetList);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
    }
}
