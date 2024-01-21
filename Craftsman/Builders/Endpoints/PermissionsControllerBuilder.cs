namespace Craftsman.Builders.Endpoints;

using System;
using Helpers;
using Services;

public class PermissionsControllerBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public PermissionsControllerBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateController(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"PermissionsController.cs", projectBaseName, "v1");
        var fileText = GetControllerFileText(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetControllerFileText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using Domain;
using HeimGuard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using {exceptionClassPath.ClassNamespace};

[ApiController]
[Route(""api/permissions"")]
[ApiVersion(""1.0"")]
public sealed class PermissionsController(IHeimGuardClient heimGuard, IUserPolicyHandler userPolicyHandler) : ControllerBase
{{
    /// <summary>
    /// Gets a list of all available permissions.
    /// </summary>
    [Authorize]
    [HttpGet(Name = ""GetPermissions"")]
    public List<string> GetPermissions()
    {{
        heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanGetPermissions);
        return Permissions.List();
    }}

    /// <summary>
    /// Gets a list of the current user's assigned permissions.
    /// </summary>
    [Authorize]
    [HttpGet(""mine"", Name = ""GetAssignedPermissions"")]
    public async Task<List<string>> GetAssignedPermissions()
    {{
        var permissions = await userPolicyHandler.GetUserPermissions();
        return permissions.ToList();
    }}
}}";
    }
}
