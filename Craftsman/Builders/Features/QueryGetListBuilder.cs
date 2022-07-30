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

    public void CreateQuery(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.GetEntityListFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetQueryFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected, permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetQueryFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
    {
        var className = FileNames.GetEntityListFeatureClassName(entity.Name);
        var queryListName = FileNames.QueryListName(entity.Name);
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var paramsDto = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";
        
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        
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
using {wrapperClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Sieve.Models;
using Sieve.Services;

public static class {className}
{{
    public class {queryListName} : IRequest<PagedList<{readDto }>>
    {{
        public readonly {paramsDto} QueryParameters;

        public {queryListName}({paramsDto} queryParameters)
        {{
            QueryParameters = queryParameters;
        }}
    }}

    public class Handler : IRequestHandler<{queryListName}, PagedList<{readDto}>>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};
        private readonly SieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}, IMapper mapper, SieveProcessor sieveProcessor{heimGuardCtor})
        {{
            _mapper = mapper;
            _{repoInterfaceProp} = {repoInterfaceProp};
            _sieveProcessor = sieveProcessor;{heimGuardSetter}
        }}

        public async Task<PagedList<{readDto}>> Handle({queryListName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var collection = _{repoInterfaceProp}.Query();

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
