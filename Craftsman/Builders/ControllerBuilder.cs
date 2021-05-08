namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public static class ControllerBuilder
    {
        public static void CreateController(string solutionDirectory, Entity entity, bool AddSwaggerComments, List<Policy> policies, string projectBaseName = "")
        {
            var classPath = ClassPathHelper.ControllerClassPath(solutionDirectory, $"{Utilities.GetControllerName(entity.Plural)}.cs", projectBaseName, "v1");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetControllerFileText(classPath.ClassNamespace, entity, AddSwaggerComments, policies, solutionDirectory, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetControllerFileText(string classNamespace, Entity entity, bool AddSwaggerComments, List<Policy> policies, string solutionDirectory, string projectBaseName)
        {
            // TODO create an attribute factory that can order them how i want and work more dynamically

            var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
            var lowercasePrimaryKey = entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var entityNamePlural = entity.Plural;
            var readDto = Utilities.GetDtoName(entityName, Dto.Read);
            var readParamDto = Utilities.GetDtoName(entityName, Dto.ReadParamaters);
            var creationDto = Utilities.GetDtoName(entityName, Dto.Creation);
            var updateDto = Utilities.GetDtoName(entityName, Dto.Update);
            var primaryKeyProp = entity.PrimaryKeyProperty;
            var queryListMethodName = Utilities.QueryListName(entityName);
            var queryRecordMethodName = Utilities.QueryRecordName(entityName);
            var addRecordCommandMethodName = Utilities.CommandAddName(entityName);
            var deleteRecordCommandMethodName = Utilities.CommandDeleteName(entityName);
            var updateRecordCommandMethodName = Utilities.CommandUpdateName(entityName);
            var patchRecordCommandMethodName = Utilities.CommandPatchName(entityName);
            var pkPropertyType = primaryKeyProp.Type;
            var listResponse = $@"Response<IEnumerable<{readDto}>>";
            var singleResponse = $@"Response<{readDto}>";
            var getListEndpointName = entity.Name == entity.Plural ? $@"Get{entityNamePlural}List" : $@"Get{entityNamePlural}";
            var getRecordEndpointName = entity.Name == entity.Plural ? $@"Get{entityNamePlural}Record" : $@"Get{entity.Name}";
            var endpointBase = Utilities.EndpointBaseGenerator(entityNamePlural);
            var getListAuthorizations = BuildAuthorizations(policies, Endpoint.GetList, entity.Name);
            var getRecordAuthorizations = BuildAuthorizations(policies, Endpoint.GetRecord, entity.Name);
            var addRecordAuthorizations = BuildAuthorizations(policies, Endpoint.AddRecord, entity.Name);
            var updateRecordAuthorizations = BuildAuthorizations(policies, Endpoint.UpdateRecord, entity.Name);
            var updatePartialAuthorizations = BuildAuthorizations(policies, Endpoint.UpdatePartial, entity.Name);
            var deleteRecordAuthorizations = BuildAuthorizations(policies, Endpoint.DeleteRecord, entity.Name);
            var hasConflictCode = entity.PrimaryKeyProperty.Type.IsGuidPropertyType();

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
    using static {featureClassPath.ClassNamespace}.{Utilities.GetEntityListFeatureClassName(entity.Name)};
    using static {featureClassPath.ClassNamespace}.{Utilities.GetEntityFeatureClassName(entity.Name)};
    using static {featureClassPath.ClassNamespace}.{Utilities.AddEntityFeatureClassName(entity.Name)};
    using static {featureClassPath.ClassNamespace}.{Utilities.DeleteEntityFeatureClassName(entity.Name)};
    using static {featureClassPath.ClassNamespace}.{Utilities.UpdateEntityFeatureClassName(entity.Name)};
    using static {featureClassPath.ClassNamespace}.{Utilities.PatchEntityFeatureClassName(entity.Name)};

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
        {GetSwaggerComments_GetList(entity, AddSwaggerComments, listResponse, getListAuthorizations.Length > 0)}{getListAuthorizations}
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpGet(Name = ""{getListEndpointName}"")]
        public async Task<IActionResult> Get{entityNamePlural}([FromQuery] {readParamDto} {lowercaseEntityVariable}ParametersDto)
        {{
            // add error handling
            var query = new {queryListMethodName}({lowercaseEntityVariable}ParametersDto);
            var queryResponse = await _mediator.Send(query);

            var paginationMetadata = new
            {{
                totalCount = queryResponse.TotalCount,
                pageSize = queryResponse.PageSize,
                currentPageSize = queryResponse.CurrentPageSize,
                currentStartIndex = queryResponse.CurrentStartIndex,
                currentEndIndex = queryResponse.CurrentEndIndex,
                pageNumber = queryResponse.PageNumber,
                totalPages = queryResponse.TotalPages,
                hasPrevious = queryResponse.HasPrevious,
                hasNext = queryResponse.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var response = new {listResponse}(queryResponse);
            return Ok(response);
        }}
        {GetSwaggerComments_GetRecord(entity, AddSwaggerComments, singleResponse, getRecordAuthorizations.Length > 0)}{getRecordAuthorizations}
        [Produces(""application/json"")]
        [HttpGet(""{{{lowercasePrimaryKey}}}"", Name = ""{getRecordEndpointName}"")]
        public async Task<ActionResult<{readDto}>> Get{entityName}({pkPropertyType} {lowercasePrimaryKey})
        {{
            // add error handling
            var query = new {queryRecordMethodName}({lowercasePrimaryKey});
            var queryResponse = await _mediator.Send(query);

            var response = new {singleResponse}(queryResponse);
            return Ok(response);
        }}
        {GetSwaggerComments_CreateRecord(entity, AddSwaggerComments, singleResponse, addRecordAuthorizations.Length > 0, hasConflictCode)}{addRecordAuthorizations}
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPost]
        public async Task<ActionResult<{readDto}>> Add{entityName}([FromBody]{creationDto} {lowercaseEntityVariable}ForCreation)
        {{
            // add error handling
            var command = new {addRecordCommandMethodName}({lowercaseEntityVariable}ForCreation);
            var commandResponse = await _mediator.Send(command);
            var response = new {singleResponse}(commandResponse);

            return CreatedAtRoute(""Get{entityName}"",
                new {{ commandResponse.{primaryKeyProp.Name} }},
                response);
        }}
        {GetSwaggerComments_DeleteRecord(entity, AddSwaggerComments, deleteRecordAuthorizations.Length > 0)}{deleteRecordAuthorizations}
        [Produces(""application/json"")]
        [HttpDelete(""{{{lowercasePrimaryKey}}}"")]
        public async Task<ActionResult> Delete{entityName}({pkPropertyType} {lowercasePrimaryKey})
        {{
            // add error handling
            var command = new {deleteRecordCommandMethodName}({lowercasePrimaryKey});
            await _mediator.Send(command);

            return NoContent();
        }}
        {GetSwaggerComments_PutRecord(entity, AddSwaggerComments, updateRecordAuthorizations.Length > 0)}{updateRecordAuthorizations}
        [Produces(""application/json"")]
        [HttpPut(""{{{lowercasePrimaryKey}}}"")]
        public async Task<IActionResult> Update{entityName}({pkPropertyType} {lowercasePrimaryKey}, {updateDto} {lowercaseEntityVariable})
        {{
            // add error handling
            var command = new {updateRecordCommandMethodName}({lowercasePrimaryKey}, {lowercaseEntityVariable});
            await _mediator.Send(command);

            return NoContent();
        }}
        {GetSwaggerComments_PatchRecord(entity, AddSwaggerComments, updatePartialAuthorizations.Length > 0)}{updatePartialAuthorizations}
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPatch(""{{{lowercasePrimaryKey}}}"")]
        public async Task<IActionResult> PartiallyUpdate{entityName}({pkPropertyType} {lowercasePrimaryKey}, JsonPatchDocument<{updateDto}> patchDoc)
        {{
            // add error handling
            var command = new {patchRecordCommandMethodName}({lowercasePrimaryKey}, patchDoc);
            await _mediator.Send(command);

            return NoContent();
        }}
    }}
}}";
        }

        private static string GetSwaggerComments_GetList(Entity entity, bool buildComments, string listResponse, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);

            if (buildComments)
                return $@"
        /// <summary>
        /// Gets a list of all {entity.Plural}.
        /// </summary>
        /// <response code=""200"">{entity.Name} list returned successfully.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        /// <remarks>
        /// Requests can be narrowed down with a variety of query string values:
        /// ## Query String Parameters
        /// - **PageNumber**: An integer value that designates the page of records that should be returned.
        /// - **PageSize**: An integer value that designates the number of records returned on the given page that you would like to return. This value is capped by the internal MaxPageSize.
        /// - **SortOrder**: A comma delimited ordered list of property names to sort by. Adding a `-` before the name switches to sorting descendingly.
        /// - **Filters**: A comma delimited list of fields to filter by formatted as `{{Name}}{{Operator}}{{Value}}` where
        ///     - {{Name}} is the name of a filterable property. You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if LikeCount or CommentCount is >10
        ///     - {{Operator}} is one of the Operators below
        ///     - {{Value}} is the value to use for filtering. You can also have multiple values (for OR logic) by using a pipe delimiter, eg.`Title@= new|hot` will return posts with titles that contain the text ""new"" or ""hot""
        ///
        ///    | Operator | Meaning                       | Operator  | Meaning                                      |
        ///    | -------- | ----------------------------- | --------- | -------------------------------------------- |
        ///    | `==`     | Equals                        |  `!@=`    | Does not Contains                            |
        ///    | `!=`     | Not equals                    |  `!_=`    | Does not Starts with                         |
        ///    | `>`      | Greater than                  |  `@=*`    | Case-insensitive string Contains             |
        ///    | `&lt;`   | Less than                     |  `_=*`    | Case-insensitive string Starts with          |
        ///    | `>=`     | Greater than or equal to      |  `==*`    | Case-insensitive string Equals               |
        ///    | `&lt;=`  | Less than or equal to         |  `!=*`    | Case-insensitive string Not equals           |
        ///    | `@=`     | Contains                      |  `!@=*`   | Case-insensitive string does not Contains    |
        ///    | `_=`     | Starts with                   |  `!_=*`   | Case-insensitive string does not Starts with |
        /// </remarks>
        [ProducesResponseType(typeof({listResponse}), 200)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        private static string GetSwaggerComments_GetRecord(Entity entity, bool buildComments, string singleResponse, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);

            if (buildComments)
                return $@"
        /// <summary>
        /// Gets a single {entity.Name} by ID.
        /// </summary>
        /// <response code=""200"">{entity.Name} record returned successfully.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(typeof({singleResponse}), 200)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        private static string GetSwaggerComments_CreateRecord(Entity entity, bool buildComments, string singleResponse, bool hasAuthentications, bool hasGuidPk)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            var conflictResponses = GetConflictResponses(hasGuidPk);
            var conflictCommentResponses = GetConflictCommentResponses(hasGuidPk);
            if (buildComments)
                return $@"
        /// <summary>
        /// Creates a new {entity.Name} record.
        /// </summary>
        /// <response code=""201"">{entity.Name} created.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}{conflictCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(typeof({singleResponse}), 201)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}{conflictResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        private static string GetSwaggerComments_DeleteRecord(Entity entity, bool buildComments, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            if (buildComments)
                return $@"
        /// <summary>
        /// Deletes an existing {entity.Name} record.
        /// </summary>
        /// <response code=""204"">{entity.Name} deleted.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        private static string GetSwaggerComments_PatchRecord(Entity entity, bool buildComments, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            if (buildComments)
                return $@"
        /// <summary>
        /// Updates specific properties on an existing {entity.Name}.
        /// </summary>
        /// <response code=""204"">{entity.Name} updated.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        private static string GetSwaggerComments_PutRecord(Entity entity, bool buildComments, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            if (buildComments)
                return $@"
        /// <summary>
        /// Updates an entire existing {entity.Name}.
        /// </summary>
        /// <response code=""204"">{entity.Name} updated.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        private static string BuildAuthorizations(List<Policy> policies, Endpoint endpoint, string entityName)
        {
            var results = Utilities.GetEndpointPolicies(policies, endpoint, entityName);

            var authorizations = "";
            foreach (var result in results)
            {
                if (result.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Scope))
                //|| result.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Claim))
                {
                    authorizations += $@"{Environment.NewLine}        [Authorize(Policy = ""{result.Name}"")]";
                }
                else
                {
                    authorizations += $@"{Environment.NewLine}        [Authorize(Roles = ""{result.Name}"")]";
                }
            }

            return authorizations;
        }

        private static string GetAuthResponses(bool hasAuthentications)
        {
            var authResponses = "";
            if (hasAuthentications)
            {
                authResponses = $@"
        [ProducesResponseType(typeof(Response<>), 401)]
        [ProducesResponseType(typeof(Response<>), 403)]";
            }

            return authResponses;
        }

        private static string GetConflictResponses(bool hasConflictResponse)
        {
            var conflictResponses = "";
            if (hasConflictResponse)
            {
                conflictResponses = $@"
        [ProducesResponseType(typeof(Response<>), 409)]";
            }

            return conflictResponses;
        }

        private static string GetAuthCommentResponses(bool hasAuthentications)
        {
            var authResponseComments = "";
            if (hasAuthentications)
            {
                authResponseComments = $@"
        /// <response code=""401"">This request was not able to be authenticated.</response>
        /// <response code=""403"">The required permissions to access this resource were not present in the given request.</response>";
            }

            return authResponseComments;
        }

        private static string GetConflictCommentResponses(bool hasConflictResponse)
        {
            var responseComments = "";
            if (hasConflictResponse)
            {
                responseComments = $@"
        /// <response code=""409"">A record already exists with this primary key.</response>";
            }

            return responseComments;
        }
    }
}