namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class QueryGetListBuilder
{
    public class Command(Entity entity, bool isProtected, string permissionName, string dbContextName) : IRequest<bool>
    {
        public Entity Entity { get; set; } = entity;
        public bool IsProtected { get; set; } = isProtected;
        public string PermissionName { get; set; } = permissionName;
        public string DbContextName { get; set; } = dbContextName;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command, bool>
    {
        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.GetEntityListFeatureClassName(request.Entity.Name)}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.IsProtected, request.PermissionName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace, Entity entity, string srcDirectory,
            string projectBaseName, bool isProtected, string permissionName, string dbContextName)
    {
        var className = FileNames.GetEntityListFeatureClassName(entity.Name);
        var queryListName = FileNames.QueryListName();
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var paramsDto = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";
        
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing);

            // lang=csharp
            return $@"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {dbContextClassPath.ClassNamespace};
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

    public sealed class Handler({dbContextName} dbContext{heimGuardCtor})
        : IRequestHandler<{queryListName}, PagedList<{readDto}>>
    {{
        public async Task<PagedList<{readDto}>> Handle({queryListName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var collection = dbContext.{entity.Plural}.AsNoTracking();

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
}
