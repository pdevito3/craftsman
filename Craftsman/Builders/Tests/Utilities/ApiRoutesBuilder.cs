namespace Craftsman.Builders.Tests.Utilities
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ApiRoutesBuilder
    {
        public static void CreateClass(string solutionDirectory, string projectBaseName, List<Entity> entities, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "ApiRoutes.cs");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetBaseText(classPath.ClassNamespace, entities);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        public static string GetBaseText(string classNamespace, List<Entity> entities)
        {
            var entityRouteClasses = "";
            foreach(var entity in entities)
            {
                var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
                var pkName = entity.PrimaryKeyProperty.Name;

                entityRouteClasses += $@"public static class {entity.Plural}
        {{
            public const string {pkName} = ""{{{pkName.LowercaseFirstLetter()}}}"";
            public const string GetList = Base + ""/{lowercaseEntityPluralName}"";
            public const string GetRecord = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
            public const string Create = Base + ""/{lowercaseEntityPluralName}"";
            public const string Delete = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
            public const string Put = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
            public const string Patch = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
        }}";
            }

            return @$"namespace {classNamespace}
{{
    public class ApiRoutes
    {{
        public const string Base = ""api"";
        public const string Health = Base + ""/health"";

        {entityRouteClasses}
    }}
}}";
        }
    }
}