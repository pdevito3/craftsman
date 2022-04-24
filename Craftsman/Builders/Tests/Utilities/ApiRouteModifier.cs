namespace Craftsman.Builders.Tests.Utilities;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class ApiRouteModifier
{
    private readonly IFileSystem _fileSystem;

    public ApiRouteModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void AddRoutes(string testDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

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

    private static string CreateApiRouteClasses(Entity entity)
    {
        var entityRouteClasses = "";

        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}public static class {entity.Plural}
    {{
        public const string {pkName} = ""{{{pkName.LowercaseFirstLetter()}}}"";
        public const string GetList = $""{{Base}}/{lowercaseEntityPluralName}"";
        public const string GetRecord = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string Create = $""{{Base}}/{lowercaseEntityPluralName}"";
        public const string Delete = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string Put = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string Patch = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string CreateBatch = $""{{Base}}/{lowercaseEntityPluralName}/batch"";
    }}";

        return entityRouteClasses;
    }
}
