namespace Craftsman.Builders.Endpoints;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetAllEndpointBuilder
{
    public static string GetEndpointTextForGetAll(Entity entity, bool addSwaggerComments, Feature feature)
    {
        var entityName = entity.Name;
        var entityNamePlural = entity.Plural;
        var readDto = FileNames.GetDtoName(entityName, Dto.Read);
        var queryListMethodName = FileNames.QueryAllName();
        var listResponse = $@"IEnumerable<{readDto}>";
        var getListEndpointName = FileNames.GetAllEntitiesFeatureClassName(entityNamePlural);
        var getListAuthorization = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations() : "";

        return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_GetList(entity, addSwaggerComments, listResponse, getListAuthorization.Length > 0)}{getListAuthorization}
    [HttpGet(""all"", Name = ""{getListEndpointName}"")]
    public async Task<IActionResult> {getListEndpointName}()
    {{
        var query = new {getListEndpointName}.{queryListMethodName}();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }}";
    }

}
