namespace Craftsman.Builders.Endpoints;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class PutRecordEndpointBuilder
{
    public static string GetEndpointText(Entity entity, bool addSwaggerComments, Feature feature)
    {
        var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
        var lowercasePrimaryKey = Entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
        var entityName = entity.Name;
        var updateDto = FileNames.GetDtoName(entityName, Dto.Update);
        var primaryKeyProp = Entity.PrimaryKeyProperty;
        var updateRecordCommandMethodName = FileNames.CommandUpdateName();
        var pkPropertyType = primaryKeyProp.Type;
        var updateRecordAuthorizations = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations() : "";

        return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_PutRecord(entity, addSwaggerComments, updateRecordAuthorizations.Length > 0)}{updateRecordAuthorizations}
    [Produces(""application/json"")]
    [HttpPut(""{{{lowercasePrimaryKey}:guid}}"", Name = ""Update{entityName}"")]
    public async Task<IActionResult> Update{entityName}({pkPropertyType} {lowercasePrimaryKey}, {updateDto} {lowercaseEntityVariable})
    {{
        var command = new {FileNames.UpdateEntityFeatureClassName(entity.Name)}.{updateRecordCommandMethodName}({lowercasePrimaryKey}, {lowercaseEntityVariable});
        await _mediator.Send(command);

        return NoContent();
    }}";
    }
}
