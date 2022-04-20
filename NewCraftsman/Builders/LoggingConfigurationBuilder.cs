namespace NewCraftsman.Builders
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class LoggingConfigurationBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public LoggingConfigurationBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }
        
        public void CreateWebApiConfigFile(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.WebApiHostExtensionsClassPath(projectDirectory, "LoggingConfiguration.cs", authServerProjectName);
            var fileText = GetConfigText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }
        
        public void CreateBffConfigFile(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.BffHostExtensionsClassPath(solutionDirectory, "LoggingConfiguration.cs", projectBaseName);
            var fileText = GetConfigTextForHostBuilder(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }
        
        private static string GetConfigText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

public static class LoggingConfiguration
{{
    public static void AddLoggingConfiguration(this IHost host)
    {{
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var env = services.GetService<IWebHostEnvironment>();

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
            .Enrich.WithEnvironment(env.EnvironmentName)
            .Enrich.WithProperty(""ApplicationName"", env.ApplicationName)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console();

        Log.Logger = logger.CreateLogger();
    }}
}}";
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
            .Enrich.WithEnvironment(env.EnvironmentName)
            .Enrich.WithProperty(""ApplicationName"", env.ApplicationName)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console();

        Log.Logger = logger.CreateLogger();
        
        host.UseSerilog();
    }}
}}";
        }
    }
}