namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class LoggingConfigurationBuilder
    {
        public static void CreateConfigFile(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiHostExtensionsClassPath(projectDirectory, "LoggingConfiguration.cs", authServerProjectName);
            var fileText = GetConfigText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetConfigText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

public static class LoggingConfiguration
{{
    public static void AddLoggingConfiguration(this IHost host)
    {{
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var env = services.GetService<IWebHostEnvironment>();
        
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override(""Microsoft"", LogEventLevel.Warning)
            .MinimumLevel.Override(""System"", LogEventLevel.Warning)
            .MinimumLevel.Override(""Microsoft.Hosting.Lifetime"", LogEventLevel.Information)
            .MinimumLevel.Override(""Microsoft.AspNetCore.Authentication"", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty(""EnvironmentName"", env.EnvironmentName)
            .Enrich.WithProperty(""ApplicationName"", env.ApplicationName)
            .Enrich.WithExceptionDetails()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .WriteTo.Console();

        if (env.IsProduction())
            logger.MinimumLevel.Error();
        
        if (env.IsDevelopment())
            logger.MinimumLevel.Debug();

        Log.Logger = logger.CreateLogger();
    }}
}}";
        }
    }
}