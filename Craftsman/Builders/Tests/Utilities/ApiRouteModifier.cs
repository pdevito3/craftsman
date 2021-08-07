namespace Craftsman.Builders.Tests.Utilities
{
    using System;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;

    public class ApiRouteModifier
    {
        public static void AddRoute(string testDirectory, Entity entity, Feature feature, string projectBaseName)
        {
            var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

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
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"new api route marker"))
                        {
                            newText += $"{Environment.NewLine}            public const string {feature.Name} = {feature.Url};";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
        
        
        public static void AddClassForRoutes(string testDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var pkName = entity.PrimaryKeyProperty.Name;
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"new api route marker"))
                        {
                            newText +=  $@"{Environment.NewLine}{Environment.NewLine}        public static class {Utilities.GetApiRouteClass(entity.Plural)}
        {{
            public const string {pkName} = ""{{{pkName.LowercaseFirstLetter()}}}"";
        }}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        public static string CreateApiRouteClasses(List<Entity> entities)
        {
            var entityRouteClasses = "";

            foreach (var entity in entities)
            {
                var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
                var pkName = entity.PrimaryKeyProperty.Name;

                entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}        public static class {entity.Plural}
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

            return entityRouteClasses;
        }
    }
}
