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

    public class QueryGetRecordBuilder
    {
        public static void CreateQuery(string solutionDirectory, Entity entity, string contextName, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, $"{Utilities.GetEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetQueryFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        public static string GetQueryFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string projectBaseName)
        {
            var className = Utilities.GetEntityFeatureClassName(entity.Name);
            var queryRecordName = Utilities.QueryRecordName(entity.Name);
            var readDto = Utilities.GetDtoName(entity.Name, Dto.Read);

            var primaryKeyPropType = entity.PrimaryKeyProperty.Type;
            var primaryKeyPropName = entity.PrimaryKeyProperty.Name;
            var primaryKeyPropNameLowercase = primaryKeyPropName.LowercaseFirstLetter();

            var fkIncludes = Utilities.GetForeignKeyIncludes(entity);

            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.CoreExceptionClassPath(solutionDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);

            return @$"namespace {classNamespace}
{{
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
        public class {queryRecordName} : IRequest<{readDto}>
        {{
            public {primaryKeyPropType} {primaryKeyPropName} {{ get; set; }}

            public {queryRecordName}({primaryKeyPropType} {primaryKeyPropNameLowercase})
            {{
                {primaryKeyPropName} = {primaryKeyPropNameLowercase};
            }}
        }}

        public class Handler : IRequestHandler<{queryRecordName}, {readDto}>
        {{
            private readonly {contextName} _db;
            private readonly IMapper _mapper;

            public Handler({contextName} db, IMapper mapper)
            {{
                _mapper = mapper;
                _db = db;
            }}

            public async Task<{readDto}> Handle({queryRecordName} request, CancellationToken cancellationToken)
            {{
                // add logger (and a try catch with logger so i can cap the unexpected info)........ unless this happens in my logger decorator that i am going to add?

                // include marker -- to accommodate adding includes with craftsman commands, the next line must stay as `var result = await _db.{entity.Plural}`. -- do not delete this comment
                var result = await _db.{entity.Plural}{fkIncludes}
                    .ProjectTo<{readDto}>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName});

                if (result == null)
                {{
                    // log error
                    throw new KeyNotFoundException();
                }}

                return result;
            }}
        }}
    }}
}}";
        }
    }
}
