namespace Craftsman.Builders;

using Helpers;
using Services;
using static Helpers.ConstMessages;

public class ProgramBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ProgramBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateWebApiProgram(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $"Program.cs", projectBaseName);
        var fileText = GetWebApiProgramText(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public void CreateAuthServerProgram(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(projectDirectory, $"Program.cs", authServerProjectName);
        var fileText = GetAuthServerProgramText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiProgramText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var hostExtClassPath = ClassPathHelper.WebApiHostExtensionsClassPath(srcDirectory, $"", projectBaseName);

        return @$"namespace {classNamespace};

using Serilog;
using System.Reflection;
using System.Threading.Tasks;
using {hostExtClassPath.ClassNamespace};

public class Program
{{
    public async static Task Main(string[] args)
    {{
        var host = CreateHostBuilder(args).Build();
        host.AddLoggingConfiguration();

        try
        {{
            Log.Information(""Starting application"");
            await host.RunAsync();
        }}
        catch (Exception e)
        {{
            Log.Error(e, ""The application failed to start correctly"");
            throw;
        }}
        finally
        {{
            Log.Information(""Shutting down application"");
            Log.CloseAndFlush();
        }}
    }}

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {{
                webBuilder.UseStartup(typeof(Startup).GetTypeInfo().Assembly.FullName)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel();
            }});
}}";
    }


    public static string GetAuthServerProgramText(string classNamespace)
    {
        return @$"{DuendeDisclosure}namespace {classNamespace};

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Serilog.Events;

public class Program
{{
    public async static Task Main(string[] args)
    {{
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override(""Microsoft"", LogEventLevel.Warning)
            .MinimumLevel.Override(""Microsoft.Hosting.Lifetime"", LogEventLevel.Information)
            .MinimumLevel.Override(""System"", LogEventLevel.Warning)
            .MinimumLevel.Override(""Microsoft.AspNetCore.Authentication"", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: ""[{{Timestamp:HH:mm:ss}} {{Level}}] {{SourceContext}}{{NewLine}}{{Message:lj}}{{NewLine}}{{Exception}}{{NewLine}}"", theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        var host = CreateHostBuilder(args).Build();

        try
        {{
            Log.Information(""Starting application"");
            await host.RunAsync();
        }}
        catch (Exception e)
        {{
            Log.Error(e, ""The application failed to start correctly"");
            throw;
        }}
        finally
        {{
            Log.Information(""Shutting down application"");
            Log.CloseAndFlush();
        }}
    }}

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {{
                webBuilder.UseStartup<Startup>();
            }});
}}";
    }
}
