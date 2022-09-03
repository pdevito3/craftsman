namespace Craftsman.Builders.Endpoints;

using System;
using Helpers;
using Services;

public class ControllerBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ControllerBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateController(string solutionDirectory, string srcDirectory, string entityName, string entityPlural, string projectBaseName, bool isProtected)
    {
        var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{FileNames.GetControllerName(entityPlural)}.cs", projectBaseName, "v1");
        var fileText = GetControllerFileText(classPath.ClassNamespace, entityPlural, solutionDirectory, srcDirectory, projectBaseName, isProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetControllerFileText(string classNamespace, string entityPlural, string solutionDirectory, string srcDirectory, string projectBaseName, bool usesJwtAuth)
    {
        // TODO create an attribute factory that can order them how i want and work more dynamically

        var endpointBase = FileNames.EndpointBaseGenerator(entityPlural);

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var featureClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");
        var permissionsUsing = usesJwtAuth
            ? @$"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};"
            : string.Empty;

        return @$"namespace {classNamespace};

using {featureClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};{permissionsUsing}
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using MediatR;

[ApiController]
[Route(""{endpointBase}"")]
[ApiVersion(""1.0"")]
public class {entityPlural}Controller: ControllerBase
{{
    private readonly IMediator _mediator;

    public {entityPlural}Controller(IMediator mediator)
    {{
        _mediator = mediator;
    }}
    
    // endpoint marker - do not delete this comment
}}";
    }
}
