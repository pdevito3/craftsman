namespace Craftsman.Builders.Features
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class CommandDeleteRecordBuilder
    {
        public static void CreateCommand(string srcDirectory, Entity entity, string contextName, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{Utilities.DeleteEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetCommandFileText(classPath.ClassNamespace, entity, contextName, srcDirectory, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string srcDirectory, string projectBaseName)
        {
            var className = Utilities.DeleteEntityFeatureClassName(entity.Name);
            var deleteCommandName = Utilities.CommandDeleteName(entity.Name);

            var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
            var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
            var entityNameLowercase = entity.Name.LowercaseFirstLetter();

            var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);

            return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public Handler({contextName} db, IMapper mapper)
        {{
            _mapper = mapper;
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
}