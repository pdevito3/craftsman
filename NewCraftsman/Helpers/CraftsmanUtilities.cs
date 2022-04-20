namespace NewCraftsman.Helpers;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Exceptions;
using Services;

public interface ICraftsmanUtilities
{
    bool ExecuteProcess(string command, string args, string directory, Dictionary<string, string> envVariables, int killInterval = 15000, string processKilledMessage = "Process Killed.");
    void ExecuteProcess(string command, string args, string directory);
    void AddProjectReference(IClassPath classPath, string relativeProjectPath);
    void CreateFile(IClassPath classPath, string fileText);
    string GetDbContext(string srcDirectory, string projectBaseName);
    void IsSolutionDirectoryGuard(string proposedDirectory);
}

public class CraftsmanUtilities : ICraftsmanUtilities
{
    private readonly IConsoleWriter _consoleWriter;
    private readonly IFileSystem _fileSystem;

    public CraftsmanUtilities(IConsoleWriter consoleWriter, IFileSystem fileSystem)
    {
        _consoleWriter = consoleWriter;
        _fileSystem = fileSystem;
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

    public void IsSolutionDirectoryGuard(string proposedDirectory)
    {
        if (!_fileSystem.Directory.EnumerateFiles(proposedDirectory, "*.sln").Any())
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
    
    public static string PropTypeCleanupTypeScript(string prop)
    {
        return prop.ToLower() switch
        {
            "boolean" => "boolean",
            "bool" => "boolean",
            "number" => "number",
            "int" => "number",
            "string" => "string",
            "dateonly" => "Date",
            "timeonly" => "Date",
            "datetimeoffset" => "Date",
            "guid" => "string",
            "uuid" => "string",
            "boolean?" => "boolean?",
            "bool?" => "boolean?",
            "number?" => "number?",
            "int?" => "number?",
            "string?" => "string?",
            "dateonly?" => "Date?",
            "timeonly?" => "Date?",
            "datetimeoffset?" => "Date?",
            "guid?" => "string?",
            "uuid?" => "string?",
            _ => prop
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

        if (_fileSystem.File.Exists(classPath.FullClassPath))
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
}

