namespace Craftsman.Builders.Tests.Utilities
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Text;

    public class ApiRoutesBuilder
    {
        public static void CreateClass(string testDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using var fs = fileSystem.File.Create(classPath.FullClassPath);
            var data = "";
            data = GetBaseText(classPath.ClassNamespace);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        private static string GetBaseText(string classNamespace)
        {
            return @$"namespace {classNamespace};
public class ApiRoutes
{{
    public const string Base = ""api"";
    public const string Health = Base + ""/health"";

    // new api route marker - do not delete
}}";
        }
    }
}