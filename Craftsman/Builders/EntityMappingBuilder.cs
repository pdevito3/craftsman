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

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {entitiesClassPath.ClassNamespace};
using Mapster;

public sealed class {FileNames.GetMappingName(entity.Name)} : IRegister
{{
    public void Register(TypeAdapterConfig config)
    {{
        config.NewConfig<{FileNames.GetDtoName(entity.Name, Dto.Read)}, {entity.Name}>()
            .TwoWays();
        config.NewConfig<{FileNames.GetDtoName(entity.Name, Dto.Creation)}, {entity.Name}>()
            .TwoWays();
        config.NewConfig<{FileNames.GetDtoName(entity.Name, Dto.Update)}, {entity.Name}>()
            .TwoWays();
    }}
}}";
    }
}
