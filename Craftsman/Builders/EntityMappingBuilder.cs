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

    public void CreateMapping(string srcDirectory, string entityName, string entityPlural, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityMappingClassPath(srcDirectory, $"{FileNames.GetMappingName(entityName)}.cs", entityPlural, projectBaseName);
        var fileText = GetMappingFileText(classPath.ClassNamespace, entityName, entityPlural, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetMappingFileText(string classNamespace, string entityName, string entityPlural, string srcDirectory, string projectBaseName)
    {
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var entityModelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entityName, entityPlural, null, projectBaseName);

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {entityModelClassPath.ClassNamespace};
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class {FileNames.GetMappingName(entityName)}
{{
    public static partial {EntityModel.Creation.GetClassName(entityName)} To{EntityModel.Creation.GetClassName(entityName)}(this {FileNames.GetDtoName(entityName, Dto.Creation)} {FileNames.GetDtoName(entityName, Dto.Creation).LowercaseFirstLetter()});
    public static partial {EntityModel.Update.GetClassName(entityName)} To{EntityModel.Update.GetClassName(entityName)}(this {FileNames.GetDtoName(entityName, Dto.Update)} {FileNames.GetDtoName(entityName, Dto.Update).LowercaseFirstLetter()});
    public static partial {FileNames.GetDtoName(entityName, Dto.Read)} To{FileNames.GetDtoName(entityName, Dto.Read)}(this {entityName} {entityName.LowercaseFirstLetter()});
    public static partial IQueryable<{FileNames.GetDtoName(entityName, Dto.Read)}> To{FileNames.GetDtoName(entityName, Dto.Read)}Queryable(this IQueryable<{entityName}> {entityName.LowercaseFirstLetter()});
}}";
    }
}
