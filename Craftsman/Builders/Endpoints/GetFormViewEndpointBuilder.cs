namespace Craftsman.Builders.Endpoints;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetFormViewEndpointBuilder
{
    public static string GetEndpointText(Entity entity, bool addSwaggerComments, Feature feature)
    {
        var lowercasePrimaryKey = Entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
        var entityName = entity.Name;
        var readDto = FileNames.GetDtoName(entityName, Dto.FormView);
        var queryRecordMethodName = FileNames.QueryFormViewName();
        var singleResponse = $@"{readDto}";
        var primaryKeyProp = Entity.PrimaryKeyProperty;
        var pkPropertyType = primaryKeyProp.Type;
        var getRecordAuthorizations = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations() : "";

        return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_GetFormView(entity, addSwaggerComments, singleResponse, getRecordAuthorizations.Length > 0)}{getRecordAuthorizations}
    [Produces(""application/json"")]
    [HttpGet(""views/form/{{{lowercasePrimaryKey}:guid}}"", Name =""{feature.Name.UppercaseFirstLetter()}"")]
    public async Task<ActionResult<{readDto}>> {feature.Name.UppercaseFirstLetter()}({pkPropertyType} {lowercasePrimaryKey})
    {{
        var query = new {FileNames.GetEntityFormViewFeatureClassName(entity.Name)}.{queryRecordMethodName}({lowercasePrimaryKey});
        var queryResponse = await _mediator.Send(query);

        return Ok(queryResponse);
    }}";
    }
}
