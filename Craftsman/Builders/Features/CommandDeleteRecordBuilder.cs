namespace Craftsman.Builders.Features;

using Domain;
using Helpers;
using Services;

public class CommandDeleteRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandDeleteRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.DeleteEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.DeleteEntityFeatureClassName(entity.Name);
        var deleteCommandName = FileNames.CommandDeleteName(entity.Name);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public static class {className}
{{
    public class {deleteCommandName} : IRequest<bool>
    {{
        public {primaryKeyPropType} {primaryKeyPropName} {{ get; set; }}

        public {deleteCommandName}({primaryKeyPropType} {entityNameLowercase})
        {{
            {primaryKeyPropName} = {entityNameLowercase};
        }}
    }}

    public class Handler : IRequestHandler<{deleteCommandName}, bool>
    {{
        private readonly {contextName} _db;

        public Handler({contextName} db)
        {{
            _db = db;
        }}

        public async Task<bool> Handle({deleteCommandName} request, CancellationToken cancellationToken)
        {{
            var recordToDelete = await _db.{entity.Plural}
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName}, cancellationToken);

            if (recordToDelete == null)
                throw new NotFoundException(""{entity.Name}"", request.{primaryKeyPropName});

            _db.{entity.Plural}.Remove(recordToDelete);
            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }}
    }}
}}";
    }
}
