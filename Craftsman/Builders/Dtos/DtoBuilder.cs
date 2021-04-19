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
        public static void CreateDtos(string solutionDirectory, Entity entity, string projectBaseName)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            CreateDtoFile(solutionDirectory, entity, Dto.Read, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Manipulation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Creation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Update, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.ReadParamaters, projectBaseName);
        }

        public static string GetDtoFileText(string solutionDirectory, ClassPath classPath, Entity entity, Dto dto, string projectBaseName)
        {
            if (dto == Dto.ReadParamaters)
                return DtoFileTextGenerator.GetReadParameterDtoText(solutionDirectory, classPath.ClassNamespace, entity, dto, projectBaseName);
            else
                return DtoFileTextGenerator.GetDtoText(classPath, entity, dto, projectBaseName);
        }

        public static void CreateDtoFile(string solutionDirectory, Entity entity, Dto dto, string projectBaseName)
        {
            var dtoFileName = $"{Utilities.GetDtoName(entity.Name, dto)}.cs";
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entity.Name, projectBaseName);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetDtoFileText(solutionDirectory, classPath, entity, dto, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }
    }
}