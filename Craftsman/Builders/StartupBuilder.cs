namespace Craftsman.Builders;

using Helpers;
using Services;
using static Helpers.ConstMessages;

public class StartupBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public StartupBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateAuthServerStartup(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.StartupClassPath(projectDirectory, authServerProjectName);
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
        _utilities.CreateFile(classPath, fileText);
    }
}
