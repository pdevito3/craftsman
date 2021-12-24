namespace Craftsman.Builders.Endpoints
{
    using System.Collections.Generic;
    using Helpers;
    using Models;

    public class DeleteRecordEndpointBuilder
    {
        public static string GetEndpointTextForDeleteRecord(Entity entity, bool addSwaggerComments, string permission)
        {
            var lowercasePrimaryKey = Entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var primaryKeyProp = Entity.PrimaryKeyProperty;
            var deleteRecordCommandMethodName = Utilities.CommandDeleteName(entityName);
            var pkPropertyType = primaryKeyProp.Type;
            var deleteRecordAuthorizations = EndpointSwaggerCommentBuilders.BuildAuthorizations(permission);

            return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_DeleteRecord(entity, addSwaggerComments, deleteRecordAuthorizations.Length > 0)}{deleteRecordAuthorizations}
    [Produces(""application/json"")]
    [HttpDelete(""{{{lowercasePrimaryKey}}}"", Name = ""Delete{entityName}"")]
    public async Task<ActionResult> Delete{entityName}({pkPropertyType} {lowercasePrimaryKey})
    {{
        var command = new {Utilities.DeleteEntityFeatureClassName(entity.Name)}.{deleteRecordCommandMethodName}({lowercasePrimaryKey});
        await _mediator.Send(command);

        return NoContent();
    }}";
        }
    }
}