namespace Craftsman.Builders;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Services;

public class WebApiLaunchSettingsModifier
{
    private readonly IFileSystem _fileSystem;

    public WebApiLaunchSettingsModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void AddProfile(string srcDirectory, ApiEnvironment env, int port, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(srcDirectory, $"launchSettings.json", projectBaseName); // hard coding webapi here not great

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains(@$"""profiles"""))
                    {
                        newText += GetProfileText(env, port);
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static string GetProfileText(ApiEnvironment env, int port)
    {
        return $@"
    ""{env.ProfileName ?? env.EnvironmentName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""{env.EnvironmentName}""
      }},
      ""applicationUrl"": ""https://localhost:{port}""
    }}";
    }

    public void UpdateLaunchSettingEnvVar(string srcDirectory, string envVarName, string envVarVal, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(srcDirectory, $"launchSettings.json", projectBaseName); // hard coding webapi here not great

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains(envVarName))
                    {
                        newText = $@"        ""{envVarName}"": ""{envVarVal}"",";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}

