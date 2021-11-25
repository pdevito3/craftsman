namespace Craftsman.Builders.Endpoints
{
    using System.Collections.Generic;
    using Enums;
    using Helpers;
    using Models;

    public class CreateRecordEndpointBuilder
    {
        public static string GetEndpointTextForCreateRecord(Entity entity, bool addSwaggerComments,List<Policy> policies)
        {
            var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var readDto = Utilities.GetDtoName(entityName, Dto.Read);
            var creationDto = Utilities.GetDtoName(entityName, Dto.Creation);
            var primaryKeyProp = Entity.PrimaryKeyProperty;
            var addRecordCommandMethodName = Utilities.CommandAddName(entityName);
            var singleResponse = $@"Response<{readDto}>";
            var addRecordAuthorizations = EndpointSwaggerCommentBuilders.BuildAuthorizations(policies);
            var creationPropName = $"{lowercaseEntityVariable}ForCreation";

            return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_CreateRecord(entity, addSwaggerComments, singleResponse, addRecordAuthorizations.Length > 0)}{addRecordAuthorizations}
    [Consumes(""application/json"")]
    [Produces(""application/json"")]
    [HttpPost(Name = ""Add{entityName}"")]
    public async Task<ActionResult<{readDto}>> Add{entityName}([FromBody]{creationDto} {creationPropName})
    {{
        var command = new {Utilities.AddEntityFeatureClassName(entity.Name)}.{addRecordCommandMethodName}({creationPropName});
        var commandResponse = await _mediator.Send(command);
        var response = new {singleResponse}(commandResponse);

        return CreatedAtRoute(""Get{entityName}"",
            new {{ commandResponse.{primaryKeyProp.Name} }},
            response);
    }}";
        }
        
        public static string GetEndpointTextForCreateList(Entity entity, bool addSwaggerComments,List<Policy> policies, Feature feature)
        {
            var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var readDto = Utilities.GetDtoName(entityName, Dto.Read);
            var creationDto = $"IEnumerable<{Utilities.GetDtoName(entityName, Dto.Creation)}>";
            var primaryKeyProp = Entity.PrimaryKeyProperty;
            var responseObj = $@"Response<IEnumerable<{readDto}>>";
            var addRecordAuthorizations = EndpointSwaggerCommentBuilders.BuildAuthorizations(policies);
            var batchPropNameLower = feature.BatchPropertyName.LowercaseFirstLetter();
            var creationPropName = $"{lowercaseEntityVariable}ForCreation";

            return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_CreateList(entity, addSwaggerComments, responseObj, addRecordAuthorizations.Length > 0)}{addRecordAuthorizations}
    [Consumes(""application/json"")]
    [Produces(""application/json"")]
    [HttpPost(Name = ""Add{entityName}List"")]
    public async Task<ActionResult<{readDto}>> Add{entityName}([FromBody]{creationDto} {creationPropName},
        [FromQuery(Name = ""{batchPropNameLower}""), BindRequired] {feature.BatchPropertyType} {batchPropNameLower})
    {{
        var command = new {feature.Name}.{feature.Command}({creationPropName}, {batchPropNameLower});
        var commandResponse = await _mediator.Send(command);
        var response = new {responseObj}(commandResponse);

        return CreatedAtRoute(""Get{entityName}"",
            new {{ {primaryKeyProp.Name} = commandResponse.Select({entity.Lambda} => {entity.Lambda}.{primaryKeyProp.Name}) }},
            response);
    }}";
        }
    }
}