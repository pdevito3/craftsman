namespace Craftsman.Builders.Endpoints
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Enums;
    using Exceptions;
    using Helpers;
    using Models;

    public static class ControllerBuilder
    {
        public static void CreateController(string solutionDirectory, string entityName, string entityPlural, string projectBaseName)
        {
            var classPath = ClassPathHelper.ControllerClassPath(solutionDirectory, $"{Utilities.GetControllerName(entityPlural)}.cs", projectBaseName, "v1");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = GetControllerFileText(classPath.ClassNamespace, entityName, entityPlural, solutionDirectory, projectBaseName);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetControllerFileText(string classNamespace, string entityName, string entityPlural, string solutionDirectory, string projectBaseName)
        {
            // TODO create an attribute factory that can order them how i want and work more dynamically

            var endpointBase = Utilities.EndpointBaseGenerator(entityPlural);

            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entityName, projectBaseName);
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(solutionDirectory, "", projectBaseName);
            var featureClassPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, "", entityPlural, projectBaseName);

            return @$"namespace {classNamespace};

using {featureClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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