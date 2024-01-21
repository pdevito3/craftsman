namespace Craftsman.Builders.Endpoints;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class PutRecordEndpointBuilder
{
    public static string GetEndpointTextForPutRecord(Entity entity, bool addSwaggerComments, Feature feature)
    {
        var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
        var lowercasePrimaryKey = $"{entity.Name.LowercaseFirstLetter()}Id";
        var entityName = entity.Name;
        var updateDto = FileNames.GetDtoName(entityName, Dto.Update);
        var primaryKeyProp = Entity.PrimaryKeyProperty;
        var updateRecordCommandMethodName = FileNames.CommandUpdateName();
        var pkPropertyType = primaryKeyProp.Type;
        var updateRecordAuthorizations = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations() : "";

        return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_PutRecord(entity, addSwaggerComments, updateRecordAuthorizations.Length > 0)}{updateRecordAuthorizations}
    [HttpPut(""{{{lowercasePrimaryKey}:guid}}"", Name = ""Update{entityName}"")]
    public async Task<IActionResult> Update{entityName}({pkPropertyType} {lowercasePrimaryKey}, {updateDto} {lowercaseEntityVariable})
    {{
        var command = new {FileNames.UpdateEntityFeatureClassName(entity.Name)}.{updateRecordCommandMethodName}({lowercasePrimaryKey}, {lowercaseEntityVariable});
        await mediator.Send(command);
        return NoContent();
    }}";
    }
}
