namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConstMessages;

    public class ProgramBuilder
    {
        public static void CreateWebApiProgram(string srcDirectory, string projectBaseName, bool useAuth, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $"Program.cs", projectBaseName);
            var fileText = GetWebApiProgramText(srcDirectory, projectBaseName, useAuth);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static void CreateAuthServerProgram(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiProjectRootClassPath(projectDirectory, $"Program.cs", authServerProjectName);
            var fileText = GetAuthServerProgramText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetWebApiProgramText(string srcDirectory, string projectBaseName, bool useAuth)
        {
            var appAuth = "";
            var hostExtClassPath = ClassPathHelper.WebApiHostExtensionsClassPath(srcDirectory, $"", projectBaseName);
            var apiServiceExtensionsClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);
            var apiAppExtensionsClassPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(srcDirectory, "", projectBaseName);
            var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var corsName = $"{projectBaseName}CorsPolicy";
            
            if (useAuth)
            {
                appAuth = $@"

        app.UseAuthentication();
        app.UseAuthorization();";
            }
            
            return @$"using {hostExtClassPath.ClassNamespace};
using {apiServiceExtensionsClassPath.ClassNamespace};
using {apiAppExtensionsClassPath.ClassNamespace};
using {dbContextClassPath.ClassNamespace};
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;
using Serilog;
using System.Reflection;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLoggingConfiguration(builder.Environment);

// Add services to the container.
builder.Services.AddSingleton(Log.Logger);
// TODO update CORS for your env
builder.Services.AddCorsService(""{corsName}"", _env);
builder.Services.AddInfrastructure(_config, _env);
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddApiVersioningExtension();
builder.Services.AddWebApiServices();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerExtension(_config);

// Configure the HTTP request pipeline.
var app = builder.Build();
if (_env.IsDevelopment())
{{
    app.UseDeveloperExceptionPage();
}}
else
{{
    app.UseExceptionHandler(""/Error"");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}}

app.UseCors(""{corsName}"");

app.UseSerilogRequestLogging();
app.UseRouting();{appAuth}

app.UseEndpoints(endpoints =>
{{
    endpoints.MapHealthChecks(""/api/health"");
    endpoints.MapControllers();
}});

app.UseSwaggerExtension(_config);

try
{{
    Log.Information(""Starting application"");
    await app.RunAsync();
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
}