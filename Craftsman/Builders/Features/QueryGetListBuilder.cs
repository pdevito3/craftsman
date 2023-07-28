namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class QueryGetListBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public QueryGetListBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateQuery(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.GetEntityListFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetQueryFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected, permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetQueryFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
    {
        var className = FileNames.GetEntityListFeatureClassName(entity.Name);
        var queryListName = FileNames.QueryListName();
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var paramsDto = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";
        var specClassName = $"{entity.Name.UppercaseFirstLetter()}WorklistSpecification";
        var specVarName = $"{entity.Name.LowercaseFirstLetter()}Specification";
        var totalCountVarName = $"total{entity.Name}Count";
        var listVarName = $"{entity.Name.LowercaseFirstLetter()}List";
        
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);

        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardSetter, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing,
            out string heimGuardField);

        return @$"namespace {classNamespace};

using Ardalis.Specification;
using {dtoClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {resourcesClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;
using QueryKit.Configuration;

public static class {className}
{{
    public sealed class {queryListName} : IRequest<PagedList<{readDto}>>
    {{
        public readonly {paramsDto} QueryParameters;

        public {queryListName}({paramsDto} queryParameters)
        {{
            QueryParameters = queryParameters;
        }}
    }}

    public sealed class Handler : IRequestHandler<{queryListName}, PagedList<{readDto}>>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}{heimGuardCtor})
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};{heimGuardSetter}
        }}

        public async Task<PagedList<{readDto}>> Handle({queryListName} request, CancellationToken cancellationToken)
        {{{permissionCheck}            
            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {{
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder ?? ""-CreatedOn"",
                Configuration = queryKitConfig
            }};

            var {specVarName} = new {specClassName}(request.QueryParameters, queryKitData);
            var {listVarName} = await _{repoInterfaceProp}.ListAsync({specVarName}, cancellationToken);
            var {totalCountVarName} = await _{repoInterfaceProp}.TotalCount(cancellationToken);

            return new PagedList<{readDto}>({listVarName}, 
                {totalCountVarName},
                request.QueryParameters.PageNumber, 
                request.QueryParameters.PageSize);
        }}

        private sealed class {specClassName} : Specification<{entity.Name}, {readDto}>
        {{
            public {specClassName}({paramsDto} parameters, QueryKitData queryKitData)
            {{
                Query.ApplyQueryKit(queryKitData)
                    .Paginate(parameters.PageNumber, parameters.PageSize)
                    .Select(x => x.To{readDto}())
                    .AsNoTracking();
            }}
        }}
    }}
}}";
    }
}
