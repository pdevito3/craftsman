namespace Craftsman.Builders.Endpoints;

using System;
using Helpers;
using Services;
using MediatR;

public static class RolesControllerBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.ControllerClassPath(scaffoldingDirectoryStore.SrcDirectory, $"RolesController.cs", scaffoldingDirectoryStore.ProjectBaseName, "v1");
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
using Domain.Roles;
using HeimGuard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using {exceptionClassPath.ClassNamespace};

[ApiController]
[Route(""api/v{{v:apiVersion}}/roles"")]
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
