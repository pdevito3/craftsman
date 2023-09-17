namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class QueryGetRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public QueryGetRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateQuery(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.GetEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetQueryFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected, permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetQueryFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
    {
        var className = FileNames.GetEntityFeatureClassName(entity.Name);
        var queryRecordName = FileNames.QueryRecordName();
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var primaryKeyPropNameLowercase = primaryKeyPropName.LowercaseFirstLetter();
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        
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
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record {queryRecordName}({primaryKeyPropType} {primaryKeyPropName}) : IRequest<{readDto}>;

    public sealed class Handler : IRequestHandler<{queryRecordName}, {readDto}>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}{heimGuardCtor})
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};{heimGuardSetter}
        }}

        public async Task<{readDto}> Handle({queryRecordName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var result = await _{repoInterfaceProp}.GetById(request.Id, cancellationToken: cancellationToken);
            return result.To{readDto}();
        }}
    }}
}}";
    }
}
