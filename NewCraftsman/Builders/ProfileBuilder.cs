namespace NewCraftsman.Builders
{
    using System.IO;
    using System.Text;
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

        public void CreateProfile(string solutionDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.ProfileClassPath(solutionDirectory, $"{FileNames.GetProfileName(entity.Name)}.cs", entity.Plural, projectBaseName);
            var fileText = GetProfileFileText(classPath.ClassNamespace, entity, solutionDirectory, projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetProfileFileText(string classNamespace, Entity entity, string solutionDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

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
        CreateMap<{FileNames.GetDtoName(entity.Name, Dto.Creation)}, {entity.Name}>();
        CreateMap<{FileNames.GetDtoName(entity.Name, Dto.Update)}, {entity.Name}>()
            .ReverseMap();
    }}
}}";
        }
    }
}