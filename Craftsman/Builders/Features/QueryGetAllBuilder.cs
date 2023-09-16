namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class QueryGetAllBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public QueryGetAllBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateQuery(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.GetAllEntitiesFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetQueryFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected, permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetQueryFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
    {
        var className = FileNames.GetAllEntitiesFeatureClassName(entity.Plural);
        var queryListName = FileNames.QueryAllName();
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";
        
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
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
    public sealed record {queryListName}() : IRequest<List<{readDto}>>;

    public sealed class Handler : IRequestHandler<{queryListName}, List<{readDto}>>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}{heimGuardCtor})
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};{heimGuardSetter}
        }}

        public async Task<List<{readDto}>> Handle({queryListName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            return _{repoInterfaceProp}.Query()
                .AsNoTracking()
                .To{FileNames.GetDtoName(entity.Name, Dto.Read)}Queryable()
                .ToList();
        }}
    }}
}}";
    }
}
