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
        public static void CreateProfile(string solutionDirectory, Entity entity)
        {
            try
            {
                var classPath = ClassPathHelper.ProfileClassPath(solutionDirectory, $"{Utilities.GetProfileName(entity.Name)}.cs");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetProfileFileText(classPath.ClassNamespace, entity);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetProfileFileText(string classNamespace, Entity entity)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};
    using AutoMapper;
    using Domain.Entities;

    public class {Utilities.GetProfileName(entity.Name)} : Profile
    {{
        public {Utilities.GetProfileName(entity.Name)}()
        {{
            //createmap<to this, from this>
            CreateMap<{entity.Name}, {Utilities.DtoNameGenerator(entity.Name,Dto.Read)}>()
                .ReverseMap();
            CreateMap<{Utilities.DtoNameGenerator(entity.Name, Dto.Creation)}, {entity.Name}>();
            CreateMap<{Utilities.DtoNameGenerator(entity.Name, Dto.Update)}, {entity.Name}>()
                .ReverseMap();
        }}
    }}
}}";
        }
    }
}
