namespace Craftsman.Builders.Endpoints
{
    using System.Collections.Generic;
    using Enums;
    using Helpers;
    using Models;

    public class PutRecordEndpointBuilder
    {
        public static string GetEndpointTextForPutRecord(Entity entity, bool addSwaggerComments,List<Policy> policies)
        {
            var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
            var lowercasePrimaryKey = Entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var updateDto = Utilities.GetDtoName(entityName, Dto.Update);
            var primaryKeyProp = Entity.PrimaryKeyProperty;
            var updateRecordCommandMethodName = Utilities.CommandUpdateName(entityName);
            var pkPropertyType = primaryKeyProp.Type;
            var updateRecordAuthorizations = EndpointSwaggerCommentBuilders.BuildAuthorizations(policies);

            return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_PutRecord(entity, addSwaggerComments, updateRecordAuthorizations.Length > 0)}{updateRecordAuthorizations}
    [Produces(""application/json"")]
    [HttpPut(""{{{lowercasePrimaryKey}:{primaryKeyProp.Type.ToLower()}}}"", Name = ""Update{entityName}"")]
    public async Task<IActionResult> Update{entityName}({pkPropertyType} {lowercasePrimaryKey}, {updateDto} {lowercaseEntityVariable})
    {{
        var command = new {Utilities.UpdateEntityFeatureClassName(entity.Name)}.{updateRecordCommandMethodName}({lowercasePrimaryKey}, {lowercaseEntityVariable});
        await _mediator.Send(command);

        return NoContent();
    }}";
        }   
    }
}