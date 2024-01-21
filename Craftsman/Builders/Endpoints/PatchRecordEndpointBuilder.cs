namespace Craftsman.Builders.Endpoints;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class PatchRecordEndpointBuilder
{
    public static string GetEndpointTextForPatchRecord(Entity entity, bool addSwaggerComments, Feature feature)
    {
        var lowercasePrimaryKey = $"{entity.Name.LowercaseFirstLetter()}Id";
        var entityName = entity.Name;
        var updateDto = FileNames.GetDtoName(entityName, Dto.Update);
        var primaryKeyProp = Entity.PrimaryKeyProperty;
        var patchRecordCommandMethodName = FileNames.CommandPatchName();
        var pkPropertyType = primaryKeyProp.Type;
        var updatePartialAuthorizations = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations() : "";

        return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_PatchRecord(entity, addSwaggerComments, updatePartialAuthorizations.Length > 0)}{updatePartialAuthorizations}
    [HttpPatch(""{{{lowercasePrimaryKey}}}"", Name = ""PartiallyUpdate{entityName}"")]
    public async Task<IActionResult> PartiallyUpdate{entityName}({pkPropertyType} {lowercasePrimaryKey}, JsonPatchDocument<{updateDto}> patchDoc)
    {{
        var command = new {FileNames.PatchEntityFeatureClassName(entity.Name)}.{patchRecordCommandMethodName}({lowercasePrimaryKey}, patchDoc);
        await mediator.Send(command);

        return NoContent();
    }}";
    }
}
