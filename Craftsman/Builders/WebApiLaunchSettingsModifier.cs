namespace Craftsman.Builders;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Services;
using MediatR;

public static class WebApiLaunchSettingsModifier
{
    public sealed record AddProfileCommand(ApiEnvironment Env, int Port) : IRequest;

    public class AddProfileHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddProfileCommand>
    {
        public Task Handle(AddProfileCommand request, CancellationToken cancellationToken)
        {
            AddProfile(scaffoldingDirectoryStore.SrcDirectory, request.Env, request.Port, scaffoldingDirectoryStore.ProjectBaseName, fileSystem);
            return Task.CompletedTask;
        }
    }

    private static void AddProfile(string srcDirectory, ApiEnvironment env, int port, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(srcDirectory, $"launchSettings.json", projectBaseName); // hard coding webapi here not great

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
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
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
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

    public sealed record UpdateLaunchSettingEnvVarCommand(string EnvVarName, string EnvVarVal) : IRequest;

    public class UpdateLaunchSettingEnvVarHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<UpdateLaunchSettingEnvVarCommand>
    {
        public Task Handle(UpdateLaunchSettingEnvVarCommand request, CancellationToken cancellationToken)
        {
            UpdateLaunchSettingEnvVar(scaffoldingDirectoryStore.SrcDirectory, request.EnvVarName, request.EnvVarVal, scaffoldingDirectoryStore.ProjectBaseName, fileSystem);
            return Task.CompletedTask;
        }
    }

    private static void UpdateLaunchSettingEnvVar(string srcDirectory, string envVarName, string envVarVal, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(srcDirectory, $"launchSettings.json", projectBaseName); // hard coding webapi here not great

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
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
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}

