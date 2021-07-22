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
            var primaryKeyProp = entity.PrimaryKeyProperty;
            var addRecordCommandMethodName = Utilities.CommandAddName(entityName);
            var singleResponse = $@"Response<{readDto}>";
            var addRecordAuthorizations = EndpointSwaggerCommentBuilders.BuildAuthorizations(policies, Endpoint.AddRecord, entity.Name);
            var hasConflictCode = entity.PrimaryKeyProperty.Type.IsGuidPropertyType();

            return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_CreateRecord(entity, addSwaggerComments, singleResponse, addRecordAuthorizations.Length > 0, hasConflictCode)}{addRecordAuthorizations}
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPost]
        public async Task<ActionResult<{readDto}>> Add{entityName}([FromBody]{creationDto} {lowercaseEntityVariable}ForCreation)
        {{
            // add error handling
            var command = new {Utilities.AddEntityFeatureClassName(entity.Name)}.{addRecordCommandMethodName}({lowercaseEntityVariable}ForCreation);
            var commandResponse = await _mediator.Send(command);
            var response = new {singleResponse}(commandResponse);

            return CreatedAtRoute(""Get{entityName}"",
                new {{ commandResponse.{primaryKeyProp.Name} }},
                response);
        }}";
        }
    }
}