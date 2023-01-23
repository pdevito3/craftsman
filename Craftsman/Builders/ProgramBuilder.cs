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

    public void CreateWebApiProgram(string srcDirectory, bool useJwtAuth, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $"Program.cs", projectBaseName);
        var fileText = GetWebApiProgramText(srcDirectory, useJwtAuth, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiProgramText(string srcDirectory, bool useJwtAuth, string projectBaseName)
    {
        var hostExtClassPath = ClassPathHelper.WebApiHostExtensionsClassPath(srcDirectory, $"", projectBaseName);
        var apiAppExtensionsClassPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(srcDirectory, "", projectBaseName);
        var configClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);
        var dbClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, $"{FileNames.GetMigrationHostedServiceFileName()}.cs", projectBaseName);
        
        var appAuth = "";
        var corsName = $"{projectBaseName}CorsPolicy";
        if (useJwtAuth)
        {
            appAuth = $@"

app.UseAuthentication();
app.UseAuthorization();";
        }

        return @$"using Serilog;
using {apiAppExtensionsClassPath.ClassNamespace};
using {hostExtClassPath.ClassNamespace};
using {configClassPath.ClassNamespace};
using {dbClassPath.ClassNamespace};

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLoggingConfiguration(builder.Environment);

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
}}

// For elevated security, it is recommended to remove this middleware and set your server to only listen on https.
// A slightly less secure option would be to redirect http to 400, 505, etc.
app.UseHttpsRedirection();

app.UseCors(""{corsName}"");

app.UseSerilogRequestLogging();
app.UseRouting();{appAuth}

app.UseEndpoints(endpoints =>
{{
    endpoints.MapHealthChecks(""/api/health"");
    endpoints.MapControllers();
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
