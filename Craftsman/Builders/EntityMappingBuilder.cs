namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class EntityMappingBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public EntityMappingBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateMapping(string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityMappingClassPath(srcDirectory, $"{FileNames.GetMappingName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetMappingFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetMappingFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName)
    {
        var entitiesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityModelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {entitiesClassPath.ClassNamespace};
using {entityModelClassPath.ClassNamespace};
using Mapster;

public sealed class {FileNames.GetMappingName(entity.Name)} : IRegister
{{
    public void Register(TypeAdapterConfig config)
    {{
        config.NewConfig<{entity.Name}, {FileNames.GetDtoName(entity.Name, Dto.Read)}>();
        config.NewConfig<{FileNames.GetDtoName(entity.Name, Dto.Creation)}, {entity.Name}>()
            .TwoWays();
        config.NewConfig<{FileNames.GetDtoName(entity.Name, Dto.Update)}, {entity.Name}>()
            .TwoWays();
        config.NewConfig<{EntityModel.Creation.GetClassName(entity.Name)}, {entity.Name}>()
            .TwoWays();
        config.NewConfig<{EntityModel.Update.GetClassName(entity.Name)}, {entity.Name}>()
            .TwoWays();
    }}
}}";
    }

    public void CreateUserMapping(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityMappingClassPath(srcDirectory, $"{FileNames.GetMappingName("User")}.cs", "Users", projectBaseName);
        var fileText = GetUserMappings(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetUserMappings(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var entitiesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Users", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "Users", projectBaseName);

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {entitiesClassPath.ClassNamespace};
using Mapster;

public sealed class UserMappings : IRegister
{{
    public void Register(TypeAdapterConfig config)
    {{
        config.NewConfig<User, UserDto>()
            .Map(x => x.Email, y => y.Email.Value);
        config.NewConfig<UserForCreationDto, User>()
            .TwoWays();
        config.NewConfig<UserForUpdateDto, User>()
            .TwoWays();
    }}
}}";
    }
}
