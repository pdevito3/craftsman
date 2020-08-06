namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ProfileBuilder
    {
        public static void CreateProfile(string solutionDirectory, Entity entity)
        {
            try
            {
                //TODO move these to a dictionary to lookup and overwrite if I want
                var validatorTopPath = $"Application\\Mappings";
                var repoNamespace = validatorTopPath.Replace("\\", ".");

                var entityDir = Path.Combine(solutionDirectory, validatorTopPath);
                if (!Directory.Exists(entityDir))
                    Directory.CreateDirectory(entityDir);

                var pathString = Path.Combine(entityDir, $"{Utilities.GetProfileName(entity)}.cs");
                if (File.Exists(pathString))
                    throw new FileAlreadyExistsException(pathString);

                using (FileStream fs = File.Create(pathString))
                {
                    var data = "";
                    data = GetProfileFileText(repoNamespace, entity);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(pathString.Replace($"{solutionDirectory}\\", ""));
                //WriteInfo($"A new '{Utilities.GetProfileName(entity)}' profile file was added here: {pathString}.");
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

    public class {Utilities.GetProfileName(entity)} : Profile
    {{
        public {Utilities.GetProfileName(entity)}()
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
