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
        public static void CreateController(string solutionDirectory, Entity entity, bool addSwaggerComments, List<Policy> policies, string projectBaseName = "")
        {
            var classPath = ClassPathHelper.ControllerClassPath(solutionDirectory, $"{Utilities.GetControllerName(entity.Plural)}.cs", projectBaseName, "v1");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = GetControllerFileText(classPath.ClassNamespace, entity, addSwaggerComments, policies, solutionDirectory, projectBaseName);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetControllerFileText(string classNamespace, Entity entity, bool addSwaggerComments, List<Policy> policies, string solutionDirectory, string projectBaseName)
        {
            // TODO create an attribute factory that can order them how i want and work more dynamically

            var entityName = entity.Name;
            var entityNamePlural = entity.Plural;
            var endpointBase = Utilities.EndpointBaseGenerator(entityNamePlural);

            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entityName, projectBaseName);
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(solutionDirectory, "", projectBaseName);
            var featureClassPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, "", entity.Plural, projectBaseName);

            return @$"namespace {classNamespace}
{{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using {dtoClassPath.ClassNamespace};
    using {wrapperClassPath.ClassNamespace};
    using System.Threading;
    using MediatR;
    using {featureClassPath.ClassNamespace};

    [ApiController]
    [Route(""{endpointBase}"")]
    [ApiVersion(""1.0"")]
    public class {entityNamePlural}Controller: ControllerBase
    {{
        private readonly IMediator _mediator;

        public {entityNamePlural}Controller(IMediator mediator)
        {{
            _mediator = mediator;
        }}
        {GetListEndpointBuilder.GetEndpointTextForGetList(entity, addSwaggerComments, policies)}
        {GetRecordEndpointBuilder.GetEndpointTextForGetRecord(entity, addSwaggerComments, policies)}
        {CreateRecordEndpointBuilder.GetEndpointTextForCreateRecord(entity, addSwaggerComments, policies)}
        {DeleteRecordEndpointBuilder.GetEndpointTextForDeleteRecord(entity, addSwaggerComments, policies)}
        {PutRecordEndpointBuilder.GetEndpointTextForPutRecord(entity, addSwaggerComments, policies)}
        {PatchRecordEndpointBuilder.GetEndpointTextForPatchRecord(entity, addSwaggerComments, policies)}
    }}
}}";
        }
    }
}