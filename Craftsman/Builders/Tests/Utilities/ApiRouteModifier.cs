namespace Craftsman.Builders.Tests.Utilities;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class ApiRouteModifier
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;

    public ApiRouteModifier(IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
    }

    public void AddRoutes(string testDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }

        var entityRouteClasses = CreateApiRouteClasses(entity);
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"new api route marker"))
                    {
                        newText += entityRouteClasses;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    public void AddRoutesForUser(string testDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var entityRouteClasses = CreateApiRouteClassesForUser();
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"new api route marker"))
                    {
                        newText += entityRouteClasses;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static string CreateApiRouteClasses(Entity entity)
    {
        var entityRouteClasses = "";

        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}    public static class {entity.Plural}
    {{
        public static string GetList => $""{{Base}}/{lowercaseEntityPluralName}"";
        public static string GetAll => $""{{Base}}/{lowercaseEntityPluralName}/all"";
        public static string GetRecord(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Delete(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Put(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Create => $""{{Base}}/{lowercaseEntityPluralName}"";
        public static string CreateBatch => $""{{Base}}/{lowercaseEntityPluralName}/batch"";
    }}";

        return entityRouteClasses;
    }

    private static string CreateApiRouteClassesForUser()
    {
        var entityRouteClasses = "";

        var lowercaseEntityPluralName = "users";
        var pkName = Entity.PrimaryKeyProperty.Name;

        entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}    public static class Users
    {{
        public static string GetList => $""{{Base}}/{lowercaseEntityPluralName}"";
        public static string GetRecord(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Delete(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Put(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Create => $""{{Base}}/{lowercaseEntityPluralName}"";
        public static string CreateBatch => $""{{Base}}/{lowercaseEntityPluralName}/batch"";
        public static string AddRole(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}/addRole"";
        public static string RemoveRole(Guid id) => $""{{Base}}/{lowercaseEntityPluralName}/{{id}}/removeRole"";
    }}";

        return entityRouteClasses;
    }
}
