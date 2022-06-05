namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandAddRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandAddRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.AddEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.AddEntityFeatureClassName(entity.Name);
        var addCommandName = FileNames.CommandAddName(entity.Name);
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var createDto = FileNames.GetDtoName(entity.Name, Dto.Creation);

        var entityName = entity.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var commandProp = $"{entityName}ToAdd";
        var newEntityProp = $"{entityNameLowercase}ToAdd";
        var repoInterface = FileNames.EntityRepositoryInterface(entityName);
        var repoInterfaceProp = $"{entityName.LowercaseFirstLetter()}Repository";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {entityServicesClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using AutoMapper;
using MediatR;

public static class {className}
{{
    public class {addCommandName} : IRequest<{readDto}>
    {{
        public readonly {createDto} {commandProp};

        public {addCommandName}({createDto} {newEntityProp})
        {{
            {commandProp} = {newEntityProp};
        }}
    }}

    public class Handler : IRequestHandler<{addCommandName}, {readDto}>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork, IMapper mapper)
        {{
            _mapper = mapper;
            _{repoInterfaceProp} = {repoInterfaceProp};
            _unitOfWork = unitOfWork;
        }}

        public async Task<{readDto}> Handle({addCommandName} request, CancellationToken cancellationToken)
        {{
            var {entityNameLowercase} = {entityName}.Create(request.{commandProp});
            await _{repoInterfaceProp}.Add({entityNameLowercase}, cancellationToken);

            await _unitOfWork.CommitChanges(cancellationToken);

            var {entityNameLowercase}Added = await _{repoInterfaceProp}.GetById({entityNameLowercase}.Id, cancellationToken: cancellationToken);
            return _mapper.Map<{readDto}>({entityNameLowercase}Added);
        }}
    }}
}}";
    }
}
