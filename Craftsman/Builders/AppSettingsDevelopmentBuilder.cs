namespace Craftsman.Builders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class AppSettingsDevelopmentBuilder
{
    public sealed record AppSettingsDevelopmentBuilderCommand(ApiEnvironment Env, DockerConfig DockerConfig) : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AppSettingsDevelopmentBuilderCommand>
    {
        public Task Handle(AppSettingsDevelopmentBuilderCommand request, CancellationToken cancellationToken)
        {
            var appSettingFilename = FileNames.GetAppSettingsName(true);
            var classPath = ClassPathHelper.WebApiAppSettingsClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{appSettingFilename}", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetAppSettingsText(request.Env, request.DockerConfig, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private static string GetAppSettingsText(ApiEnvironment env, DockerConfig dockerConfig, string projectBaseName)
        {
            var cleanName = CraftsmanUtilities.GetCleanProjectName(projectBaseName);
            // lang=json
            return 
$$"""
{
  "AllowedHosts": "*",
  "{{cleanName}}": {
    "ConnectionStrings": {
      "{{CraftsmanUtilities.GetCleanProjectName(projectBaseName)}}": "{{dockerConfig.DbConnectionString}}"
    },
    "Auth": {
      "Audience": "{{env.AuthSettings.Audience}}",
      "Authority": "{{env.AuthSettings.Authority}}",
      "AuthorizationUrl": "{{env.AuthSettings.AuthorizationUrl}}",
      "TokenUrl": "{{env.AuthSettings.TokenUrl}}",
      "ClientId": "{{env.AuthSettings.ClientId}}",
      "ClientSecret": "{{env.AuthSettings.ClientSecret}}"
    },
    "RabbitMq": {
      "Host": "{{env.BrokerSettings.Host}}",
      "VirtualHost": "{{env.BrokerSettings.VirtualHost}}",
      "Username": "{{env.BrokerSettings.Username}}",
      "Password": "{{env.BrokerSettings.Password}}",
      "Port": "{{env.BrokerSettings.BrokerPort}}"
    },
    "JaegerHost": "localhost"
  }
}
""";
        }
    }
}
