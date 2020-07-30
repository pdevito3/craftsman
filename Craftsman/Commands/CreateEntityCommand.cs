namespace Craftsman.Commands
{
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static ConsoleWriter;

    public static class CreateEntityCommand
    {
        public static void CreateEntity(string rootProjectDirectory, Entity entity)
        {
            try
            {
                WriteInfo($"Starting the creation of the '{entity.Name}' entity.");

                var modelsTopDir = "Models";
                var modelDir = Path.Combine(rootProjectDirectory, modelsTopDir);
                if (!Directory.Exists(modelDir))
                    Directory.CreateDirectory(modelDir);

                var pathString = Path.Combine(modelDir, $"{entity.Name.UppercaseFirstLetter()}.cs");

                if (File.Exists(pathString))
                    throw new FileAlreadyExistsException();

                using (FileStream fs = File.Create(pathString))
                {
                    var projectNamspace = $"{new DirectoryInfo(rootProjectDirectory).Name}.{modelsTopDir}";
                    var data = GetEntityFileText(projectNamspace, entity);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                WriteInfo($"A new '{entity.Name}' entity file was added here: {pathString}.");
            }
            catch(FileAlreadyExistsException)
            {
                WriteError("This file alread exists. Please enter a valid file path.");
                throw;
            }
            catch(Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string GetEntityFileText(string classNamespace, Entity entity)
        {
            var propString = PropBuilder(entity.Properties);
            return @$"namespace {classNamespace}
{{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class {entity.Name}
    {{
{propString}
    }}
}}";
        }

        private static string PropBuilder(List<EntityProperty> props)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                string newLine = eachProp == props.Count ? "" : Environment.NewLine;
                propString += $@"        public {PropTypeCleanup(props[eachProp].Type)} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }

        private static string PropTypeCleanup(string prop)
        {
            var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "byte", "char", "byte", "ushort", "uint", "ulong" };
            if (lowercaseProps.Contains(prop.ToLower()))
                return prop.ToLower();
            else if (prop.ToLower() == "datetime")
                return "DateTime";
            else if (prop.ToLower() == "datetime?")
                return "DateTime?";
            else
                return prop;
        }
    }
}
