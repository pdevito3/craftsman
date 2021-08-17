namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class QueryGetRecordBuilder
    {
        public static void CreateQuery(string solutionDirectory, Entity entity, string contextName, string projectBaseName)
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

        public static string GetQueryFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string projectBaseName)
        {
            var className = Utilities.GetEntityFeatureClassName(entity.Name);
            var queryRecordName = Utilities.QueryRecordName(entity.Name);
            var readDto = Utilities.GetDtoName(entity.Name, Dto.Read);

            var primaryKeyPropType = entity.PrimaryKeyProperty.Type;
            var primaryKeyPropName = entity.PrimaryKeyProperty.Name;
            var primaryKeyPropNameLowercase = primaryKeyPropName.LowercaseFirstLetter();

            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "", projectBaseName);
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

    public static class {className}
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
                var result = await _db.{entity.Plural}
                    .ProjectTo<{readDto}>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName});

                if (result == null)
                    throw new KeyNotFoundException();

                return result;
            }}
        }}
    }}
}}";
        }
    }
}