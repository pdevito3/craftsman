namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class ProgramBuilder
{
    public sealed record ProgramBuilderCommand(bool UseJwtAuth, bool UseCustomErrorHandler) : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<ProgramBuilderCommand>
    {
        public Task Handle(ProgramBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiProjectRootClassPath(scaffoldingDirectoryStore.SrcDirectory, $"Program.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetWebApiProgramText(scaffoldingDirectoryStore.SrcDirectory, request.UseJwtAuth, scaffoldingDirectoryStore.ProjectBaseName, request.UseCustomErrorHandler);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
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

        return @$"using Destructurama;
using Serilog;
using Hangfire;{errorUsingStatement}
using {apiAppExtensionsClassPath.ClassNamespace};
using {configClassPath.ClassNamespace};
using {dbClassPath.ClassNamespace};
using {hangfireUtilsClassPath.ClassNamespace};

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty(""ApplicationName"", builder.Environment.ApplicationName)
    .Destructure.UsingAttributes()
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
}
