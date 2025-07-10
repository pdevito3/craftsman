namespace Craftsman.Builders.Endpoints;

using System;
using Helpers;
using Services;
using MediatR;

public static class PermissionsControllerBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.ControllerClassPath(scaffoldingDirectoryStore.SrcDirectory, $"PermissionsController.cs", scaffoldingDirectoryStore.ProjectBaseName, "v1");
            var fileText = GetControllerFileText(classPath.ClassNamespace, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetControllerFileText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using Asp.Versioning;
using Domain;
using HeimGuard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using {exceptionClassPath.ClassNamespace};

[ApiController]
[Route(""api/v{{v:apiVersion}}/permissions"")]
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
