namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ProfileBuilder
    {
        public static void CreateProfile(string solutionDirectory, Entity entity, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.ProfileClassPath(solutionDirectory, $"{Utilities.GetProfileName(entity.Name)}.cs", entity.Plural, projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetProfileFileText(classPath.ClassNamespace, entity, solutionDirectory, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetProfileFileText(string classNamespace, Entity entity, string solutionDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            return @$"namespace {classNamespace}
{{
    using {dtoClassPath.ClassNamespace};
    using AutoMapper;
    using {entitiesClassPath.ClassNamespace};

    public class {Utilities.GetProfileName(entity.Name)} : Profile
    {{
        public {Utilities.GetProfileName(entity.Name)}()
        {{
            //createmap<to this, from this>
            CreateMap<{entity.Name}, {Utilities.GetDtoName(entity.Name,Dto.Read)}>()
                .ReverseMap();
            CreateMap<{Utilities.GetDtoName(entity.Name, Dto.Creation)}, {entity.Name}>();
            CreateMap<{Utilities.GetDtoName(entity.Name, Dto.Update)}, {entity.Name}>()
                .ReverseMap();
        }}
    }}
}}";
        }
    }
}
