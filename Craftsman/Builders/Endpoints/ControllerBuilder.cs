namespace Craftsman.Builders.Endpoints;

using System;
using Helpers;
using MediatR;
using Services;

public static class ControllerBuilder
{
    public sealed record ControllerBuilderCommand(string EntityPlural, string ProjectBaseName, bool IsProtected) : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<ControllerBuilderCommand>
    {
        public Task Handle(ControllerBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.ControllerClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.GetControllerName(request.EntityPlural)}.cs", request.ProjectBaseName, "v1");
            var fileText = GetControllerFileText(classPath.ClassNamespace, request.EntityPlural, scaffoldingDirectoryStore.SrcDirectory, request.ProjectBaseName, request.IsProtected);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetControllerFileText(string classNamespace, string entityPlural, string srcDirectory, string projectBaseName, bool usesJwtAuth)
    {
        // TODO create an attribute factory that can order them how i want and work more dynamically

        var endpointBase = FileNames.EndpointBaseGenerator(entityPlural);

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var featureClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var permissionsUsing = usesJwtAuth
            ? @$"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};"
            : string.Empty;

        return @$"namespace {classNamespace};

using {featureClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {resourcesClassPath.ClassNamespace};{permissionsUsing}
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using Asp.Versioning;
using MediatR;

[ApiController]
[Route(""{endpointBase}"")]
[ApiVersion(""1.0"")]
public sealed class {entityPlural}Controller(IMediator mediator): ControllerBase
{{    
    // endpoint marker - do not delete this comment
}}";
    }
}
