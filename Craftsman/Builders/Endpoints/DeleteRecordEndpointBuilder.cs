namespace Craftsman.Builders.Endpoints
{
    using System.Collections.Generic;
    using Helpers;
    using Models;

    public class DeleteRecordEndpointBuilder
    {
        public static string GetEndpointTextForDeleteRecord(Entity entity, bool addSwaggerComments,List<Policy> policies)
        {
            var lowercasePrimaryKey = entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var primaryKeyProp = entity.PrimaryKeyProperty;
            var deleteRecordCommandMethodName = Utilities.CommandDeleteName(entityName);
            var pkPropertyType = primaryKeyProp.Type;
            var deleteRecordAuthorizations = EndpointSwaggerCommentBuilders.BuildAuthorizations(policies, Endpoint.DeleteRecord, entity.Name);

            return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_DeleteRecord(entity, addSwaggerComments, deleteRecordAuthorizations.Length > 0)}{deleteRecordAuthorizations}
        [Produces(""application/json"")]
        [HttpDelete(""{{{lowercasePrimaryKey}}}"", Name = ""Delete{entityName}"")]
        public async Task<ActionResult> Delete{entityName}({pkPropertyType} {lowercasePrimaryKey})
        {{
            // add error handling
            var command = new {Utilities.DeleteEntityFeatureClassName(entity.Name)}.{deleteRecordCommandMethodName}({lowercasePrimaryKey});
            await _mediator.Send(command);

            return NoContent();
        }}";
        }
    }
}