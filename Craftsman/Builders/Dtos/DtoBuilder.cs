namespace Craftsman.Builders.Dtos
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public static class DtoBuilder
    {
        public static void CreateDtos(string solutionDirectory, Entity entity)
        {
            try
            {
                // ****this class path will have an invalid FullClassPath. just need the directory
                var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                CreateDtoFile(solutionDirectory, entity, Dto.Read);
                CreateDtoFile(solutionDirectory, entity, Dto.Manipulation);
                CreateDtoFile(solutionDirectory, entity, Dto.Creation);
                CreateDtoFile(solutionDirectory, entity, Dto.Update);
                CreateDtoFile(solutionDirectory, entity, Dto.ReadParamaters);
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

        public static string GetDtoFileText(ClassPath classPath, Entity entity, Dto dto)
        {
            if (dto == Dto.ReadParamaters)
                return DtoFileTextGenerator.GetReadParameterDtoText(classPath.ClassNamespace, entity, dto);
            else
                return DtoFileTextGenerator.GetDtoText(classPath, entity, dto);
        }

        public static void CreateDtoFile(string solutionDirectory, Entity entity, Dto dto)
        {
            var dtoFileName = $"{Utilities.GetDtoName(entity.Name, dto)}.cs";
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entity.Name);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetDtoFileText(classPath, entity, dto);
                fs.Write(Encoding.UTF8.GetBytes(data));

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
        }
    }
}
