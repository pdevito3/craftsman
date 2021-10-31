namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConstMessages;

    public class StartupBuilder
    {
        public static void CreateWebApiStartup(string srcDirectory, bool useJwtAuth, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = Utilities.GetStartupClassPath(srcDirectory, projectBaseName);
            var fileText = GetWebApiStartupText(srcDirectory, classPath.ClassNamespace, useJwtAuth, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateAuthServerStartup(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = Utilities.GetStartupClassPath(projectDirectory, authServerProjectName);
            var testUsersClassPath = ClassPathHelper.AuthServerSeederClassPath(projectDirectory, "", authServerProjectName);
            
            var fileText = @$"{DuendeDisclosure}namespace {classPath.ClassNamespace};
using Duende.IdentityServer;
using Microsoft.AspNetCore.Builder;
using {testUsersClassPath.ClassNamespace};

public class Startup
{{
    public IConfiguration _config {{ get; }}
    public IWebHostEnvironment _env {{ get; }}

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {{
        _config = configuration;
        _env = env;
    }}

    public void ConfigureServices(IServiceCollection services)
    {{
        services.AddControllersWithViews();

        var identityServerBuilder = services.AddIdentityServer(options =>
        {{
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            // see https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
            options.EmitStaticAudienceClaim = true;
        }});

        if(_env.IsDevelopment())
        {{
            identityServerBuilder.AddTestUsers(TestUsers.Users);
            identityServerBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
            identityServerBuilder.AddInMemoryApiScopes(Config.ApiScopes);
            identityServerBuilder.AddInMemoryApiResources(Config.ApiResources); // this is the new api resource registration
            identityServerBuilder.AddInMemoryClients(Config.Clients);
        }}

        services.AddAuthentication();
    }}

    public void Configure(IApplicationBuilder app)
    {{
        if (_env.IsDevelopment())
        {{
            app.UseDeveloperExceptionPage();
        }}

        app.UseStaticFiles();

        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {{
            endpoints.MapDefaultControllerRoute();
        }});
    }}
}}";
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetWebApiStartupText(string solutionDirectory, string classNamespace, bool useJwtAuth, string projectBaseName)
        {
            var appAuth = "";
            var apiServiceExtensionsClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, "", projectBaseName);
            var apiAppExtensionsClassPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(solutionDirectory, "", projectBaseName);
            var seederClassPath = ClassPathHelper.DummySeederClassPath(solutionDirectory, "", projectBaseName);

            if (useJwtAuth)
            {
                appAuth = $@"

        app.UseAuthentication();
        app.UseAuthorization();";
            }

            var dbContextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var corsName = $"{projectBaseName}CorsPolicy";

            return @$"namespace {classNamespace};

using {apiServiceExtensionsClassPath.ClassNamespace};
using {apiAppExtensionsClassPath.ClassNamespace};
using {seederClassPath.ClassNamespace};
using {dbContextClassPath.ClassNamespace};
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

public class Startup
{{
    public IConfiguration _config {{ get; }}
    public IWebHostEnvironment _env {{ get; }}

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {{
        _config = configuration;
        _env = env;
    }}

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {{
        services.AddSingleton(Log.Logger);
        // TODO update CORS for your env
        services.AddCorsService(""{corsName}"", _env);
        services.AddInfrastructure(_config, _env);
        services.AddControllers()
            .AddNewtonsoftJson(o => o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        services.AddApiVersioningExtension();
        services.AddWebApiServices();
        services.AddHealthChecks();

        // Dynamic Services
    }}

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {{
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

        // For elevated security, it is recommended to remove this middleware and set your server to only listen on https.
        // A slightly less secure option would be to redirect http to 400, 505, etc.
        app.UseHttpsRedirection();

        app.UseCors(""{corsName}"");

        app.UseSerilogRequestLogging();
        app.UseRouting();{appAuth}

        app.UseErrorHandlingMiddleware();
        app.UseEndpoints(endpoints =>
        {{
            endpoints.MapHealthChecks(""/api/health"");
            endpoints.MapControllers();
        }});

        // Dynamic App
    }}
}}";
        }
    }
}