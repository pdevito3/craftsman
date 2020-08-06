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
                //TODO move these to a dictionary to lookup and overwrite if I want
                var entityTopPath = $"Application\\Dtos\\{entity.Name}";
                var entityNamespace = entityTopPath.Replace("\\", ".");

                var entityDir = Path.Combine(solutionDirectory, entityTopPath);
                if (!Directory.Exists(entityDir))
                    Directory.CreateDirectory(entityDir);

                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Read);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Manipulation);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Creation);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Update);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.PaginationParamaters);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.ReadParamaters);
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

        public static string GetDtoFileText(string classNamespace, Entity entity, Dto dto)
        {
            if (dto == Dto.PaginationParamaters)
                return DtoFileTextGenerator.GetPaginationDtoText(classNamespace, entity, dto);
            else if (dto == Dto.ReadParamaters)
                return DtoFileTextGenerator.GetReadParameterDtoText(classNamespace, entity, dto);
            else
                return DtoFileTextGenerator.GetDtoText(classNamespace, entity, dto);
        }

        public static void CreateDtoFile(string entityDir, string entityNamespace, Entity entity, Dto dto)
        {
            var dtoFileName = Utilities.DtoNameGenerator(entity.Name, dto);
            var pathString = Path.Combine(entityDir, $"{dtoFileName}.cs");
            if (File.Exists(pathString))
                throw new FileAlreadyExistsException(pathString);
            using (FileStream fs = File.Create(pathString))
            {
                var data = GetDtoFileText(entityNamespace, entity, dto);
                fs.Write(Encoding.UTF8.GetBytes(data));
                WriteInfo($"A new '{dtoFileName}' file was added here: {pathString}.");
            }
        }
    }
}
