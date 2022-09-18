namespace Craftsman.Helpers;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Domain;
using Domain.Enums;
using Exceptions;
using Services;
using Spectre.Console;

public interface ICraftsmanUtilities
{
    bool ExecuteProcess(string command, string args, string directory, Dictionary<string, string> envVariables, int killInterval = 15000, string processKilledMessage = "Process Killed.");
    void ExecuteProcess(string command, string args, string directory);
    void AddProjectReference(IClassPath classPath, string relativeProjectPath);
    void CreateFile(IClassPath classPath, string fileText);
    string GetDbContext(string srcDirectory, string projectBaseName);
    void IsSolutionDirectoryGuard(string proposedDirectory, bool slnIsInParent = false);
    bool ProjectUsesSoftDelete(string srcDirectory, string projectBaseName);
    string GetRootDir();
    void IsBoundedContextDirectoryGuard();
    void AddPackages(ClassPath classPath, Dictionary<string, string> packagesToAdd);
}

public class CraftsmanUtilities : ICraftsmanUtilities
{
    private readonly IConsoleWriter _consoleWriter;
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

    public CraftsmanUtilities(IConsoleWriter consoleWriter, IFileSystem fileSystem, IAnsiConsole console, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
    {
        _consoleWriter = consoleWriter;
        _fileSystem = fileSystem;
        _console = console;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
    }

    public string GetDbContext(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.DbContextClassPath(srcDirectory, $"", projectBaseName);
        var directoryClasses = _fileSystem.Directory.GetFiles(classPath.FullClassPath, "*.cs");
        foreach (var directoryClass in directoryClasses)
        {
            using var input = _fileSystem.File.OpenText(directoryClass);
            string line;
            while (null != (line = input.ReadLine()))
            {
                if (line.Contains($": DbContext"))
                    return _fileSystem.Path.GetFileNameWithoutExtension(directoryClass);
            }
        }

        return "";
    }

    public void IsSolutionDirectoryGuard(string proposedDirectory, bool slnIsInParent = false)
    {
        if (_fileSystem.Directory.EnumerateFiles(proposedDirectory, "*.sln").Any())
            return;

        if(slnIsInParent) 
            throw new SolutionNotFoundException("A solution file was not found in the parent directory. You might need to go down one level to your boundary directory (has 'src' and 'test' directories) and run your command there instead.");

        throw new SolutionNotFoundException();
    }

    public void CreateFile(IClassPath classPath, string fileText)
    {
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileAlreadyExistsException(classPath.FullClassPath);

        using var fs = _fileSystem.File.Create(classPath.FullClassPath);
        fs.Write(Encoding.UTF8.GetBytes(fileText));
    }

    public static int GetFreePort()
    {
        // From https://stackoverflow.com/a/150974/4190785
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }

    public static string GetForeignEntityUsings(string testDirectory, Entity entity,
        string projectBaseName)
    {
        var foreignEntityUsings = "";
        var foreignProps = entity.Properties.Where(e => e.IsForeignKey).ToList();
        foreach (var entityProperty in foreignProps)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany)
            {
                var parentClassPath =
                    ClassPathHelper.TestFakesClassPath(testDirectory, $"", entityProperty.ForeignEntityName, projectBaseName);

                foreignEntityUsings += $@"
using {parentClassPath.ClassNamespace};";
            }
        }

        return foreignEntityUsings;
    }

    public static string PropTypeCleanupDotNet(string prop)
    {
        var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "char", "byte", "ushort", "uint", "ulong" };
        if (lowercaseProps.Contains(prop.ToLower()))
            return prop.ToLower();

        return prop.ToLower() switch
        {
            "datetime" => "DateTime",
            "datetime?" => "DateTime?",
            "dateonly?" => "DateOnly?",
            "dateonly" => "DateOnly",
            "timeonly?" => "TimeOnly?",
            "timeonly" => "TimeOnly",
            "datetimeoffset" => "DateTimeOffset",
            "datetimeoffset?" => "DateTimeOffset?",
            "guid" => "Guid",
            _ => prop
        };
    }

    public static TypescriptPropertyType PropTypeCleanupTypeScript(string prop)
    {
        return prop.ToLower() switch
        {
            "boolean" => TypescriptPropertyType.BooleanProperty,
            "bool" => TypescriptPropertyType.BooleanProperty,
            "number" => TypescriptPropertyType.NumberProperty,
            "int" => TypescriptPropertyType.NumberProperty,
            "string" => TypescriptPropertyType.StringProperty,
            "datetime" => TypescriptPropertyType.DateProperty,
            "dateonly" => TypescriptPropertyType.DateProperty,
            "timeonly" => TypescriptPropertyType.DateProperty,
            "datetimeoffset" => TypescriptPropertyType.DateProperty,
            "guid" => TypescriptPropertyType.StringProperty,
            "uuid" => TypescriptPropertyType.StringProperty,
            "boolean?" => TypescriptPropertyType.NullableBooleanProperty,
            "bool?" => TypescriptPropertyType.NullableBooleanProperty,
            "number?" => TypescriptPropertyType.NullableNumberProperty,
            "int?" => TypescriptPropertyType.NullableNumberProperty,
            "string?" => TypescriptPropertyType.NullableStringProperty,
            "dateonly?" => TypescriptPropertyType.NullableDateProperty,
            "timeonly?" => TypescriptPropertyType.NullableDateProperty,
            "datetimeoffset?" => TypescriptPropertyType.NullableDateProperty,
            "guid?" => TypescriptPropertyType.NullableStringProperty,
            "uuid?" => TypescriptPropertyType.NullableStringProperty,
            _ => prop.Contains('?') ? TypescriptPropertyType.NullableOther : TypescriptPropertyType.Other
        };
    }

    public bool ExecuteProcess(string command, string args, string directory, Dictionary<string, string> envVariables, int killInterval = 15000, string processKilledMessage = "Process Killed.")
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = directory
            }
        };

        process.StartInfo.EnvironmentVariables[envVariables.Keys.FirstOrDefault()] = envVariables.Values.FirstOrDefault();

        process.Start();
        if (!process.WaitForExit(killInterval))
        {
            process.Kill();
            _consoleWriter.WriteWarning(processKilledMessage);
            return false;
        }
        return true;
    }

    public void ExecuteProcess(string command, string args, string directory)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = directory
            }
        };

        process.Start();
        process.WaitForExit();
    }

    public void AddProjectReference(IClassPath classPath, string relativeProjectPath)
    {
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            string line;
            var projectAdded = false;
            while (null != (line = input.ReadLine()))
            {
                var newText = $"{line}";
                if (line.Contains($"</Project>") && !projectAdded)
                {
                    newText = @$"
  <ItemGroup>
    <ProjectReference Include=""{relativeProjectPath}"" />
  </ItemGroup>

{newText}";
                    projectAdded = true;
                }

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    public bool ProjectUsesSoftDelete(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"BaseEntity.cs", "", projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            return false;

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return false;

        using var input = _fileSystem.File.OpenText(classPath.FullClassPath);
        string line;
        while (null != (line = input.ReadLine()))
        {
            if (line.Contains($"Deleted"))
                return true;
        }

        return false;
    }

    public string GetRootDir()
    {
        var rootDir = _fileSystem.Directory.GetCurrentDirectory();
        var myEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (myEnv == "Dev")
            rootDir = _console.Ask<string>("Enter the root directory of your project:");
        return rootDir;
    }

    public void IsBoundedContextDirectoryGuard()
    {
        if (!_fileSystem.Directory.Exists(_scaffoldingDirectoryStore.SrcDirectory) || !Directory.Exists(_scaffoldingDirectoryStore.TestDirectory))
            throw new IsNotBoundedContextDirectoryException();
    }

    public void AddPackages(ClassPath classPath, Dictionary<string, string> packagesToAdd)
    {
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            string line;
            var packagesAdded = false;
            while (null != (line = input.ReadLine()))
            {
                var newText = $"{line}";
                if (line.Contains($"PackageReference") && !packagesAdded)
                {
                    newText += @$"{ProjectReferencePackagesString(packagesToAdd)}";
                    packagesAdded = true;
                }

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static string ProjectReferencePackagesString(Dictionary<string, string> packagesToAdd)
    {
        var packageString = "";
        foreach (var package in packagesToAdd)
        {
            packageString += $@"{Environment.NewLine}    <PackageReference Include=""{package.Key}"" Version=""{package.Value}"" />";
        }

        return packageString;
    }
}

