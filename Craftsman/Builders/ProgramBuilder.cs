namespace Craftsman.Builders;

using Helpers;
using Services;

public class ProgramBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ProgramBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateWebApiProgram(string srcDirectory, bool useJwtAuth, string projectBaseName, bool useCustomErrorHandler)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $"Program.cs", projectBaseName);
        var fileText = GetWebApiProgramText(srcDirectory, useJwtAuth, projectBaseName, useCustomErrorHandler);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiProgramText(string srcDirectory, bool useJwtAuth, string projectBaseName, bool useCustomErrorHandler)
    {
        var hostExtClassPath = ClassPathHelper.WebApiHostExtensionsClassPath(srcDirectory, $"", projectBaseName);
        var apiAppExtensionsClassPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(srcDirectory, "", projectBaseName);
        var configClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);
        var dbClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, $"{FileNames.GetMigrationHostedServiceFileName()}.cs", projectBaseName);
        var hangfireUtilsClassPath = ClassPathHelper.HangfireResourcesClassPath(srcDirectory, $"", projectBaseName);
        
        var errorUsingStatement = !useCustomErrorHandler ? $@"{Environment.NewLine}using Hellang.Middleware.ProblemDetails;" : ""; 
        var errorRegistration = !useCustomErrorHandler ? $"{Environment.NewLine}app.UseProblemDetails();" : "";
        var appAuth = useJwtAuth ? $@"{Environment.NewLine}{Environment.NewLine}app.UseAuthentication();
app.UseAuthorization();" : "";
        var corsName = $"{projectBaseName}CorsPolicy";

        return @$"using Serilog;
using Hangfire;{errorUsingStatement}
using {apiAppExtensionsClassPath.ClassNamespace};
using {hostExtClassPath.ClassNamespace};
using {configClassPath.ClassNamespace};
using {dbClassPath.ClassNamespace};
using {hangfireUtilsClassPath.ClassNamespace};

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLoggingConfiguration(builder.Environment);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty(""ApplicationName"", builder.Environment.ApplicationName)
    .CreateLogger();

builder.Host.UseSerilog();

builder.ConfigureServices();
var app = builder.Build();

using var scope = app.Services.CreateScope();
if (builder.Environment.IsDevelopment())
{{
    app.UseDeveloperExceptionPage();
}}
else
{{
    app.UseExceptionHandler(""/Error"");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}}{errorRegistration}

// For elevated security, it is recommended to remove this middleware and set your server to only listen on https.
// A slightly less secure option would be to redirect http to 400, 505, etc.
app.UseHttpsRedirection();

app.UseCors(""{corsName}"");

app.MapHealthChecks(""api/health"");
app.UseSerilogRequestLogging();
app.UseRouting();{appAuth}

app.MapHealthChecks(""api/health"");
app.MapControllers();

app.UseHangfireDashboard(""/hangfire"", new DashboardOptions
{{
    AsyncAuthorization = new[] {{ new HangfireAuthorizationFilter(scope.ServiceProvider) }},
    IgnoreAntiforgeryToken = true
}});

app.UseSwaggerExtension(builder.Configuration, builder.Environment);

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
}}

// Make the implicit Program class public so the functional test project can access it
public partial class Program {{ }}";
    }
}
