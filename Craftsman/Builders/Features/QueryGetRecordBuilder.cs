namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class QueryGetRecordBuilder
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
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.GetEntityFeatureClassName(request.Entity.Name)}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.IsProtected, request.PermissionName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace, Entity entity, string srcDirectory, 
            string projectBaseName, bool isProtected, string permissionName, string dbContextName)
    {
        var className = FileNames.GetEntityFeatureClassName(entity.Name);
        var queryRecordName = FileNames.QueryRecordName();
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var lowercasePrimaryKey = $"{entity.Name}Id";

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
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
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

public static class {className}
{{
    public sealed record {queryRecordName}({primaryKeyPropType} {lowercasePrimaryKey}) : IRequest<{readDto}>;

    public sealed class Handler({dbContextName} dbContext{heimGuardCtor})
        : IRequestHandler<{queryRecordName}, {readDto}>
    {{
        public async Task<{readDto}> Handle({queryRecordName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var result = await dbContext.{entity.Plural}
                .AsNoTracking()
                .GetById(request.{lowercasePrimaryKey}, cancellationToken);
            return result.To{readDto}();
        }}
    }}
}}";
        }
    }
}
