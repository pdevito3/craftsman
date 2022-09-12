namespace Craftsman.Builders.Endpoints;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetListViewEndpointBuilder
{
    public static string GetEndpointText(Entity entity, bool addSwaggerComments, Feature feature)
    {
        var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
        var entityName = entity.Name;
        var readDto = FileNames.GetDtoName(entityName, Dto.ListView);
        var readParamDto = FileNames.GetDtoName(entityName, Dto.ListViewParameters);
        var queryListMethodName = FileNames.QueryListViewName();
        var listResponse = $@"IEnumerable<{readDto}>";
        var getListAuthorization = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations() : "";

        return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_GetList(entity, addSwaggerComments, listResponse, getListAuthorization.Length > 0)}{getListAuthorization}
    [Produces(""application/json"")]
    [HttpGet(""views/list"", Name = ""{feature.Name.UppercaseFirstLetter()}"")]
    public async Task<IActionResult> {feature.Name.UppercaseFirstLetter()}([FromQuery] {readParamDto} {lowercaseEntityVariable}ParametersDto)
    {{
        var query = new {FileNames.GetEntityListViewFeatureClassName(entity.Name)}.{queryListMethodName}({lowercaseEntityVariable}ParametersDto);
        var queryResponse = await _mediator.Send(query);

        var paginationMetadata = new
        {{
            totalCount = queryResponse.TotalCount,
            pageSize = queryResponse.PageSize,
            currentPageSize = queryResponse.CurrentPageSize,
            currentStartIndex = queryResponse.CurrentStartIndex,
            currentEndIndex = queryResponse.CurrentEndIndex,
            pageNumber = queryResponse.PageNumber,
            totalPages = queryResponse.TotalPages,
            hasPrevious = queryResponse.HasPrevious,
            hasNext = queryResponse.HasNext
        }};

        Response.Headers.Add(""X-Pagination"",
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(queryResponse);
    }}";
    }

}
