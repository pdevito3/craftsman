namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class QueryGetListBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public QueryGetListBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateQuery(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.GetEntityListFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetQueryFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetQueryFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.GetEntityListFeatureClassName(entity.Name);
        var queryListName = FileNames.QueryListName(entity.Name);
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var paramsDto = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Sieve.Models;
using Sieve.Services;
using System.Threading;
using System.Threading.Tasks;

public static class {className}
{{
    public class {queryListName} : IRequest<PagedList<{readDto }>>
    {{
        public {paramsDto} QueryParameters {{ get; set; }}

        public {queryListName}({paramsDto} queryParameters)
        {{
            QueryParameters = queryParameters;
        }}
    }}

    public class Handler : IRequestHandler<{queryListName}, PagedList<{readDto}>>
    {{
        private readonly {contextName} _db;
        private readonly SieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public Handler({contextName} db, IMapper mapper, SieveProcessor sieveProcessor)
        {{
            _mapper = mapper;
            _db = db;
            _sieveProcessor = sieveProcessor;
        }}

        public async Task<PagedList<{readDto}>> Handle({queryListName} request, CancellationToken cancellationToken)
        {{
            var collection = _db.{entity.Plural}
                as IQueryable<{entity.Name}>;

            var sieveModel = new SieveModel
            {{
                Sorts = request.QueryParameters.SortOrder ?? ""{primaryKeyPropName}"",
                Filters = request.QueryParameters.Filters
            }};

            var appliedCollection = _sieveProcessor.Apply(sieveModel, collection);
            var dtoCollection = appliedCollection
                .ProjectTo<{readDto}>(_mapper.ConfigurationProvider);

            return await PagedList<{readDto}>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }}
    }}
}}";
    }
}
