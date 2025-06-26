namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class LoggingConfigurationBuilder
{
    public sealed record CreateWebApiConfigFileCommand(string ProjectDirectory, string AuthServerProjectName) : IRequest;
    public sealed record CreateBffConfigFileCommand(string SolutionDirectory, string ProjectBaseName) : IRequest;

    public class CreateWebApiConfigFileHandler(ICraftsmanUtilities utilities) : IRequestHandler<CreateWebApiConfigFileCommand>
    {
        public Task Handle(CreateWebApiConfigFileCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiHostExtensionsClassPath(request.ProjectDirectory, "LoggingConfiguration.cs", request.AuthServerProjectName);
            var fileText = GetConfigTextForHostBuilder(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public class CreateBffConfigFileHandler(ICraftsmanUtilities utilities) : IRequestHandler<CreateBffConfigFileCommand>
    {
        public Task Handle(CreateBffConfigFileCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.BffHostExtensionsClassPath(request.SolutionDirectory, "LoggingConfiguration.cs", request.ProjectBaseName);
            var fileText = GetConfigTextForHostBuilder(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetConfigTextForHostBuilder(string classNamespace)
    {
        return @$"namespace {classNamespace};

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

public static class LoggingConfiguration
{{
    public static void AddLoggingConfiguration(this IHostBuilder host, IWebHostEnvironment env)
    {{
        var loggingLevelSwitch = new LoggingLevelSwitch();
        if (env.IsDevelopment())
            loggingLevelSwitch.MinimumLevel = LogEventLevel.Warning;
        if (env.IsProduction())
            loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
        
        var logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(loggingLevelSwitch)
            .MinimumLevel.Override(""Microsoft.Hosting.Lifetime"", LogEventLevel.Information)
            .MinimumLevel.Override(""Microsoft.AspNetCore.Authentication"", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty(""EnvironmentName"", env.EnvironmentName)
            .Enrich.WithProperty(""ApplicationName"", env.ApplicationName)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console();

        Log.Logger = logger.CreateLogger();
        
        host.UseSerilog();
    }}
}}";
    }
}
