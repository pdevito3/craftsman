namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class ProfileBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ProfileBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateProfile(string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.ProfileClassPath(srcDirectory, $"{FileNames.GetProfileName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetProfileFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetProfileFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName)
    {
        var entitiesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using AutoMapper;
using {entitiesClassPath.ClassNamespace};

public class {FileNames.GetProfileName(entity.Name)} : Profile
{{
    public {FileNames.GetProfileName(entity.Name)}()
    {{
        //createmap<to this, from this>
        CreateMap<{entity.Name}, {FileNames.GetDtoName(entity.Name, Dto.Read)}>()
            .ReverseMap();
        CreateMap<{entity.Name}, {FileNames.GetDtoName(entity.Name, Dto.Creation)}>()
            .ReverseMap();
        CreateMap<{entity.Name}, {FileNames.GetDtoName(entity.Name, Dto.Update)}>()
            .ReverseMap();
    }}
}}";
    }
}
