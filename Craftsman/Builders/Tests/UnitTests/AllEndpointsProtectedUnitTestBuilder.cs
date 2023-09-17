namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Helpers;
using Services;

public class AllEndpointsProtectedUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AllEndpointsProtectedUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestProjectGuardsTestsClassPath(testDirectory, $"EndpointTests.cs", projectBaseName);
        var fileText = WriteTestFileText(classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(ClassPath classPath, string projectBaseName)
    {
        return @$"namespace {classPath.ClassNamespace};

using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using TestHelpers;

public sealed class EndpointTests
{{
    /// <summary>
    /// Turn on this test to check that all endpoints are protected by an Authorize attribute.
    /// </summary>
    // [Fact]
    public void can_protect_all_endpoints_except_opt_outs()
    {{
        // Arrange
        var endpoints = GetEndpointsFromProject().ToList();
        var unprotectedEndpoints = new List<string>()
        {{
            // Add endpoints that are deliberately not protected here
        }};
        endpoints = endpoints.Where(x => !unprotectedEndpoints.Contains(x.Name)).ToList();

        // Act
        var unauthenticatedEndpoints = endpoints.Where(e => !e.RequiresAuthentication);

        // Assert
        unauthenticatedEndpoints.Should().BeEmpty(""All endpoints should require authorization, unless explicitly exempt."");
    }}

    private static IEnumerable<Endpoint> GetEndpointsFromProject()
    {{
        var apiAssembly = UnitTestUtils.GetApiAssembly();
        var controllers = apiAssembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Controller)) || t.IsSubclassOf(typeof(ControllerBase)));

        var endpoints = controllers.SelectMany(controller => controller.GetMethods())
            .Where(method => method.IsPublic && method.IsDefined(typeof(HttpMethodAttribute)));

        return endpoints.Select(endpoint => new Endpoint
        {{
            Name = endpoint.Name,
            RequiresAuthentication = endpoint.IsDefined(typeof(AuthorizeAttribute))
        }});
    }}

    private sealed class Endpoint
    {{
        public string Name {{ get; set; }}
        public bool RequiresAuthentication {{ get; set; }}
    }}
}}";
    }
}
