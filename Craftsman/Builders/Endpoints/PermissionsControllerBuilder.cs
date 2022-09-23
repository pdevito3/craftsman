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
        var fileText = GetControllerFileText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetControllerFileText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using Domain;
using HeimGuard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Exceptions;

[ApiController]
[Route(""api/permissions"")]
[ApiVersion(""1.0"")]
public sealed class PermissionsController: ControllerBase
{{
    private readonly IHeimGuardClient _heimGuard;
    private readonly IUserPolicyHandler _userPolicyHandler;

    public PermissionsController(IHeimGuardClient heimGuard, IUserPolicyHandler userPolicyHandler)
    {{
        _heimGuard = heimGuard;
        _userPolicyHandler = userPolicyHandler;
    }}

    /// <summary>
    /// Gets a list of all available permissions.
    /// </summary>
    /// <response code=""200"">List retrieved.</response>
    /// <response code=""500"">There was an error getting the list of permissions.</response>
    [Authorize]
    [HttpGet(Name = ""GetPermissions"")]
    public List<string> GetPermissions()
    {{
        _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanGetPermissions);
        return Permissions.List();
    }}

    /// <summary>
    /// Gets a list of the current user's assigned permissions.
    /// </summary>
    /// <response code=""200"">List retrieved.</response>
    /// <response code=""500"">There was an error getting the list of permissions.</response>
    [Authorize]
    [HttpGet(""mine"", Name = ""GetAssignedPermissions"")]
    public async Task<List<string>> GetAssignedPermissions()
    {{
        var permissions = await _userPolicyHandler.GetUserPermissions();
        return permissions.ToList();
    }}
}}";
    }
}
