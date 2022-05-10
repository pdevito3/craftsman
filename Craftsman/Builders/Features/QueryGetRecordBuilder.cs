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

    public void CreateQuery(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.GetEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetQueryFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetQueryFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.GetEntityFeatureClassName(entity.Name);
        var queryRecordName = FileNames.QueryRecordName(entity.Name);
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var primaryKeyPropNameLowercase = primaryKeyPropName.LowercaseFirstLetter();

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

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
                .AsNoTracking()
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName}, cancellationToken);

            if (result == null)
                throw new NotFoundException(""{entity.Name}"", request.{primaryKeyPropName});

            return _mapper.Map<{readDto}>(result);
        }}
    }}
}}";
    }
}
