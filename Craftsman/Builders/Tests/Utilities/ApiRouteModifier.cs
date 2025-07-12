namespace Craftsman.Builders.Tests.Utilities;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;
using MediatR;

public static class ApiRouteModifier
{
    public sealed record AddRoutesCommand(Entity Entity, string ProjectBaseName) : IRequest;
    public sealed record AddRoutesForUserCommand(string ProjectBaseName) : IRequest;

    public class AddRoutesHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddRoutesCommand>
    {
        public Task Handle(AddRoutesCommand request, CancellationToken cancellationToken)
        {
            AddRoutes(scaffoldingDirectoryStore.TestDirectory, request.Entity, request.ProjectBaseName, fileSystem, consoleWriter);
            return Task.CompletedTask;
        }
    }

    public class AddRoutesForUserHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddRoutesForUserCommand>
    {
        public Task Handle(AddRoutesForUserCommand request, CancellationToken cancellationToken)
        {
            AddRoutesForUser(scaffoldingDirectoryStore.TestDirectory, request.ProjectBaseName, fileSystem, consoleWriter);
            return Task.CompletedTask;
        }
    }

    private static void AddRoutes(string testDirectory, Entity entity, string projectBaseName, IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
        {
            consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }

        var entityRouteClasses = CreateApiRouteClasses(entity);
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
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
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static void AddRoutesForUser(string testDirectory, string projectBaseName, IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var entityRouteClasses = CreateApiRouteClassesForUser();
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
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
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static string CreateApiRouteClasses(Entity entity)
    {
        var entityRouteClasses = "";

        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}    public static class {entity.Plural}
    {{
        public static string GetList(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string GetAll(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/all"";
        public static string GetRecord(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Delete(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Put(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Create(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string CreateBatch(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/batch"";
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
        public static string GetList(string version = ""v1"")  => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string GetRecord(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Delete(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Put(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Create(string version = ""v1"")  => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string CreateBatch(string version = ""v1"")  => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/batch"";
        public static string AddRole(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}/addRole"";
        public static string RemoveRole(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}/removeRole"";
    }}";

        return entityRouteClasses;
    }
}
