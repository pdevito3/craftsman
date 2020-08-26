namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
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

    public class DtoModifier
    {
        public static void AddPropertiesToDtos(string solutionDirectory, string entityName, List<EntityProperty> props)
        {
            UpdateDtoFile(solutionDirectory, entityName, props, Dto.Read);
            UpdateDtoFile(solutionDirectory, entityName, props, Dto.Manipulation);
        }

        private static void UpdateDtoFile(string solutionDirectory, string entityName, List<EntityProperty> props, Dto dto)
        {
            var dtoFileName = $"{Utilities.GetDtoName(entityName, dto)}.cs";
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entityName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    var fkUsingStatements = "";

                    if (dto == Dto.Read)
                    { 
                        foreach (var prop in props)
                        {
                            fkUsingStatements += DtoFileTextGenerator.GetForeignKeyUsingStatements(classPath, fkUsingStatements, prop, dto);
                        }
                    }

                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"add-on property marker"))
                        {
                            newText += @$"{Environment.NewLine}{Environment.NewLine}{DtoFileTextGenerator.DtoPropBuilder(props, dto)}";
                        }
                        if (line.Contains("using System;"))
                        {
                            newText += @$"{Environment.NewLine}    using Application.Dtos.Product;";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
        }
    }
}
