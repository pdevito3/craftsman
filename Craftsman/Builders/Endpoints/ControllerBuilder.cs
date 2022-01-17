namespace Craftsman.Builders.Endpoints
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using Enums;
    using Exceptions;
    using Helpers;
    using Models;

    public static class ControllerBuilder
    {
        public static void CreateController(string solutionDirectory, string srcDirectory, string entityName, string entityPlural, string projectBaseName, bool isProtected, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{Utilities.GetControllerName(entityPlural)}.cs", projectBaseName, "v1");
            var fileText = GetControllerFileText(classPath.ClassNamespace, entityName, entityPlural, solutionDirectory, srcDirectory, projectBaseName, isProtected);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetControllerFileText(string classNamespace, string entityName, string entityPlural, string solutionDirectory, string srcDirectory, string projectBaseName, bool usesJwtAuth)
        {
            // TODO create an attribute factory that can order them how i want and work more dynamically

            var endpointBase = Utilities.EndpointBaseGenerator(entityPlural);

            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entityName, projectBaseName);
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
            var featureClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, "", entityPlural, projectBaseName);
            var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
            var permissionsUsing = usesJwtAuth 
                ? $"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};"
                : string.Empty;

            return @$"namespace {classNamespace};

using {featureClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};{permissionsUsing}
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
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
}