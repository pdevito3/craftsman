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
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";
        
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        
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

using {dtoClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {resourcesClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class {className}
{{
    public sealed record {queryListName}({paramsDto} QueryParameters) : IRequest<PagedList<{readDto }>>;

    public sealed class Handler : IRequestHandler<{queryListName}, PagedList<{readDto}>>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}{heimGuardCtor})
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};{heimGuardSetter}
        }}

        public async Task<PagedList<{readDto}>> Handle({queryListName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var collection = _{repoInterfaceProp}.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {{
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            }};
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.To{readDto}Queryable();

            return await PagedList<{readDto}>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }}
    }}
}}";
    }
}
