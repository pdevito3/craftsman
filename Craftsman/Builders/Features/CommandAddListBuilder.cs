namespace Craftsman.Builders.Features;

using System;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class CommandAddListBuilder
{
    public sealed record Command(Entity Entity, Feature Feature, bool IsProtected, string PermissionName, string DbContextName) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.Feature.Name}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, request.Feature, scaffoldingDirectoryStore.ProjectBaseName, request.IsProtected, request.PermissionName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, Feature feature, string projectBaseName, bool isProtected, string permissionName, string dbContextName)
    {
        var className = feature.Name;
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var readDtoAsList = $"List<{readDto}>";
        var createDto = FileNames.GetDtoName(entity.Name, Dto.Creation);
        createDto = $"IEnumerable<{createDto}>";
        var featurePropNameLowerFirst = feature.BatchPropertyName.LowercaseFirstLetter();

        var entityName = entity.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var parentEntityNameLowercaseFirst = feature.ParentEntity.LowercaseFirstLetter();
        var entityNameLowercaseListVar = $"{entity.Name.LowercaseFirstLetter()}List";
        var commandProp = $"{entityName}ListToAdd";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);
        var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing);

        var batchFkCheck = !string.IsNullOrEmpty(feature.BatchPropertyName)
            ? @$"
            var {parentEntityNameLowercaseFirst} = await dbContext.{feature.ParentEntityPlural}.GetById(command.{feature.BatchPropertyName}, cancellationToken);{Environment.NewLine}{Environment.NewLine}            "
            : "";

        return @$"namespace {classNamespace};

using {dbContextClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record Command({createDto} {commandProp}, {feature.BatchPropertyType} {feature.BatchPropertyName}) : IRequest<{readDtoAsList}>;

    public sealed class Handler({dbContextName} dbContext{heimGuardCtor})
        : IRequestHandler<Command, {readDtoAsList}>
    {{
        public async Task<{readDtoAsList}> Handle(Command command, CancellationToken cancellationToken)
        {{{permissionCheck}{batchFkCheck}var {entityNameLowercaseListVar}ToAdd = command.{commandProp}.ToList();
            var {entityNameLowercaseListVar} = new List<{entityName}>();
            foreach (var {entityNameLowercase} in {entityNameLowercaseListVar}ToAdd)
            {{
                var {entityNameLowercase}ForCreation = {entityNameLowercase}.To{EntityModel.Creation.GetClassName(entity.Name)}();
                var {entityNameLowercase}ToAdd = {entityName}.Create({entityNameLowercase}ForCreation);
                {entityNameLowercaseListVar}.Add({entityNameLowercase}ToAdd);
                {parentEntityNameLowercaseFirst}.Add{entityName}({entityNameLowercase}ToAdd);
            }}

            // if you have large datasets to add in bulk and have performance concerns, there 
            // are additional methods that could be leveraged in your repository instead (e.g. SqlBulkCopy)
            // https://timdeschryver.dev/blog/faster-sql-bulk-inserts-with-csharp#table-valued-parameter 
            await dbContext.{entity.Plural}.AddRangeAsync({entityNameLowercaseListVar}, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return {entityNameLowercaseListVar}
                .Select({entity.Lambda} => {entity.Lambda}.To{readDto}())
                .ToList();
        }}
    }}
}}";
    }
}