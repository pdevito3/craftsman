namespace Craftsman.Builders.Endpoints
{
    using Domain;
    using Domain.Enums;
    using Helpers;
    using Services;

    public class GetRecordEndpointBuilder
    {
        public static string GetEndpointTextForGetRecord(Entity entity, bool addSwaggerComments, Feature feature)
        {
            var lowercasePrimaryKey = Entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var entityNamePlural = entity.Plural;
            var readDto = FileNames.GetDtoName(entityName, Dto.Read);
            var primaryKeyProp = Entity.PrimaryKeyProperty;
            var queryRecordMethodName = FileNames.QueryRecordName(entityName);
            var pkPropertyType = primaryKeyProp.Type;
            var singleResponse = $@"{readDto}";
            var getRecordEndpointName = entity.Name == entity.Plural ? $@"Get{entityNamePlural}Record" : $@"Get{entity.Name}";
            var getRecordAuthorizations = feature.IsProtected ? EndpointSwaggerCommentBuilders.BuildAuthorizations(feature.PermissionName) : "";


            return @$"{EndpointSwaggerCommentBuilders.GetSwaggerComments_GetRecord(entity, addSwaggerComments, singleResponse, getRecordAuthorizations.Length > 0)}{getRecordAuthorizations}
    [Produces(""application/json"")]
    [HttpGet(""{{{lowercasePrimaryKey}:guid}}"", Name = ""{getRecordEndpointName}"")]
    public async Task<ActionResult<{readDto}>> Get{entityName}({pkPropertyType} {lowercasePrimaryKey})
    {{
        var query = new {FileNames.GetEntityFeatureClassName(entity.Name)}.{queryRecordMethodName}({lowercasePrimaryKey});
        var queryResponse = await _mediator.Send(query);

        return Ok(queryResponse);
    }}";
        }
    }
}