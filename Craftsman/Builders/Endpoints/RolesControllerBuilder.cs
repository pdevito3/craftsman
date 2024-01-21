namespace Craftsman.Builders.Endpoints;

using System;
using Helpers;
using Services;

public class RolesControllerBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public RolesControllerBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateController(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"RolesController.cs", projectBaseName, "v1");
        var fileText = GetControllerFileText(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetControllerFileText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using Domain;
using Domain.Roles;
using HeimGuard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using {exceptionClassPath.ClassNamespace};

[ApiController]
[Route(""api/roles"")]
[ApiVersion(""1.0"")]
public sealed class RolesController(IHeimGuardClient heimGuard): ControllerBase
{{
    /// <summary>
    /// Gets a list of all available roles.
    /// </summary>
    [Authorize]
    [HttpGet(Name = ""GetRoles"")]
    public List<string> GetRoles()
    {{
        heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanGetRoles);
        return Role.ListNames();
    }}
}}";
    }
}
