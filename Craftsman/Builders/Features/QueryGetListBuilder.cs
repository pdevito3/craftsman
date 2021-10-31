namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class QueryGetListBuilder
    {
        public static void CreateQuery(string solutionDirectory, Entity entity, string contextName, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, $"{Utilities.GetEntityListFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);

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
            var className = Utilities.GetEntityListFeatureClassName(entity.Name);
            var queryListName = Utilities.QueryListName(entity.Name);
            var readDto = Utilities.GetDtoName(entity.Name, Dto.Read);
            var paramsDto = Utilities.GetDtoName(entity.Name, Dto.ReadParamaters);
            var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;

            var entityClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(solutionDirectory, "", projectBaseName);

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
            if (request.QueryParameters == null)
                throw new ApiException(""Invalid query parameters."");

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
                request.QueryParameters.PageSize);
        }}
    }}
}}";
        }
    }
}