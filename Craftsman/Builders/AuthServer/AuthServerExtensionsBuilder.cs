namespace Craftsman.Builders.AuthServer;

using Helpers;
using Services;
using static Helpers.ConstMessages;

public class AuthServerExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AuthServerExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateExtensions(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.AuthServerExtensionsClassPath(projectDirectory, "Extensions.cs", authServerProjectName);
        var fileText = GetExtensionsText(classPath.ClassNamespace, projectDirectory, authServerProjectName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetExtensionsText(string classNamespace, string projectDirectory, string authServerProjectName)
    {
        var viewModelClassPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "", authServerProjectName);

        return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace};

using Microsoft.AspNetCore.Mvc;
using Duende.IdentityServer.Models;
using {viewModelClassPath.ClassNamespace};

public static class Extensions
{{
    /// <summary>
    /// Checks if the redirect URI is for a native client.
    /// </summary>
    /// <returns></returns>
    public static bool IsNativeClient(this AuthorizationRequest context)
    {{
        return !context.RedirectUri.StartsWith(""https"", StringComparison.Ordinal)
           && !context.RedirectUri.StartsWith(""http"", StringComparison.Ordinal);
    }}

    public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
    {{
        controller.HttpContext.Response.StatusCode = 200;
        controller.HttpContext.Response.Headers[""Location""] = """";
        
        return controller.View(viewName, new RedirectViewModel {{ RedirectUrl = redirectUri }});
    }}
}}";
    }
}
