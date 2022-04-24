namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandUpdateRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandUpdateRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.UpdateEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.UpdateEntityFeatureClassName(entity.Name);
        var updateCommandName = FileNames.CommandUpdateName(entity.Name);
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var commandProp = $"{entity.Name}ToUpdate";
        var newEntityDataProp = $"new{entity.Name}Data";
        var updatedEntityProp = $"{entityNameLowercase}ToUpdate";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var validatorsClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {validatorsClassPath.ClassNamespace};
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public static class {className}
{{
    public class {updateCommandName} : IRequest<bool>
    {{
        public {primaryKeyPropType} {primaryKeyPropName} {{ get; set; }}
        public {updateDto} {commandProp} {{ get; set; }}

        public {updateCommandName}({primaryKeyPropType} {entityNameLowercase}, {updateDto} {newEntityDataProp})
        {{
            {primaryKeyPropName} = {entityNameLowercase};
            {commandProp} = {newEntityDataProp};
        }}
    }}

    public class Handler : IRequestHandler<{updateCommandName}, bool>
    {{
        private readonly {contextName} _db;
        private readonly IMapper _mapper;

        public Handler({contextName} db, IMapper mapper)
        {{
            _mapper = mapper;
            _db = db;
        }}

        public async Task<bool> Handle({updateCommandName} request, CancellationToken cancellationToken)
        {{
            var {updatedEntityProp} = await _db.{entity.Plural}
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName}, cancellationToken);

            if ({updatedEntityProp} == null)
                throw new NotFoundException(""{entity.Name}"", request.{primaryKeyPropName});

            {updatedEntityProp}.Update(request.{commandProp});
            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }}
    }}
}}";
    }
}
