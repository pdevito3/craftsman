namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class CommandDeleteRecordBuilder
    {
        public static void CreateCommand(string solutionDirectory, Entity entity, string contextName, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, $"{Utilities.DeleteEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string projectBaseName)
        {
            var className = Utilities.DeleteEntityFeatureClassName(entity.Name);
            var deleteCommandName = Utilities.CommandDeleteName(entity.Name);

            var primaryKeyPropType = entity.PrimaryKeyProperty.Type;
            var primaryKeyPropName = entity.PrimaryKeyProperty.Name;
            var entityNameLowercase = entity.Name.LowercaseFirstLetter();

            var entityClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.CoreExceptionClassPath(solutionDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);

            return @$"namespace {classNamespace}
{{
    using {entityClassPath.ClassNamespace};
    using {dtoClassPath.ClassNamespace};
    using {exceptionsClassPath.ClassNamespace};
    using {contextClassPath.ClassNamespace};
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class {className}
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
                // add logger (and a try catch with logger so i can cap the unexpected info)........ unless this happens in my logger decorator that i am going to add?

                var recordToDelete = await _db.{entity.Plural}
                    .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName});

                if (recordToDelete == null)
                {{
                    // log error
                    throw new KeyNotFoundException();
                }}

                _db.{entity.Plural}.Remove(recordToDelete);
                var saveSuccessful = await _db.SaveChangesAsync() > 0;

                if (!saveSuccessful)
                {{
                    // add log
                    throw new Exception(""Unable to save the new record. Please check the logs for more information."");
                }}

                return true;
            }}
        }}
    }}
}}";
        }
    }
}
