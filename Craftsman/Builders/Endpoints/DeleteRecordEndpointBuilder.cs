﻿namespace Craftsman.Builders.Endpoints;

using Domain;
using Helpers;
using Services;

public class DeleteRecordEndpointBuilder
{
    public static string GetEndpointTextForDeleteRecord(Entity entity, bool addSwaggerComments, Feature feature)
    {
        var lowercasePrimaryKey = Entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
        var entityName = entity.Name;
        var primaryKeyProp = Entity.PrimaryKeyProperty;
        var deleteRecordCommandMethodName = FileNames.CommandDeleteName();
        var pkPropertyType = primaryKeyProp.Type;
        var deleteRecordAuthorizations = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations() : "";

        return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_DeleteRecord(entity, addSwaggerComments, deleteRecordAuthorizations.Length > 0)}{deleteRecordAuthorizations}
    [HttpDelete(""{{{lowercasePrimaryKey}:guid}}"", Name = ""Delete{entityName}"")]
    public async Task<ActionResult> Delete{entityName}({pkPropertyType} {lowercasePrimaryKey})
    {{
        var command = new {FileNames.DeleteEntityFeatureClassName(entity.Name)}.{deleteRecordCommandMethodName}({lowercasePrimaryKey});
        await _mediator.Send(command);

        return NoContent();
    }}";
    }
}
