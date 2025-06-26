namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class AppSettingsBuilder
{
    public sealed record AppSettingsBuilderCommand(string DbName) : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AppSettingsBuilderCommand>
    {
        public Task Handle(AppSettingsBuilderCommand request, CancellationToken cancellationToken)
        {
            var appSettingFilename = FileNames.GetAppSettingsName();
            var classPath = ClassPathHelper.WebApiAppSettingsClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{appSettingFilename}", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetAppSettingsText();
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private static string GetAppSettingsText()
        {
            // lang=json
            return $$"""
                     {
                       "AllowedHosts": "*",
                       "Serilog": {
                         "MinimiumLevel": {
                           "Default": "Information",
                           "Override": {
                             "Microsoft.Hosting.Lifetime": "Information",
                             "Microsoft.AspNetCore.Authentication": "Information"
                           }
                         },
                         "Enrich": [
                           "FromLogContext", 
                           "WithExceptionDetails",
                           "WithMachineName",
                           "WithThreadId"
                         ],
                         "WriteTo": [
                           {
                             "Name": "Console"
                           }
                         ]
                       }
                     }
                     """;
        }
    }
}
