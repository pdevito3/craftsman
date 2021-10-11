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
        public static void CreateWebApiStartup(string srcDirectory, string envName, bool useJwtAuth, string projectBaseName)
        {
            var classPath = Utilities.GetStartupClassPath(envName, srcDirectory, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetStartupText(srcDirectory, classPath.ClassNamespace, envName, useJwtAuth, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static void CreateAuthServerStartup(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = Utilities.GetStartupClassPath(null, projectDirectory, authServerProjectName);
            var testUsersClassPath = ClassPathHelper.AuthServerSeederClassPath(projectDirectory, "", authServerProjectName);
            
            var fileText = @$"{DuendeDisclosure}namespace {classPath.ClassNamespace}
{{
    using Duende.IdentityServer;
    using IdentityServerHost.Quickstart.UI;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
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
    }}
}}";
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetStartupText(string solutionDirectory, string classNamespace, string envName, bool useJwtAuth, string projectBaseName)
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

            envName = envName == "Production" ? "" : envName;
            if (envName == "Development")
                return @$"namespace {classNamespace}
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using {seederClassPath.ClassNamespace};
    using {dbContextClassPath.ClassNamespace};
    using {apiServiceExtensionsClassPath.ClassNamespace};
    using {apiAppExtensionsClassPath.ClassNamespace};
    using Serilog;

    public class Startup{envName}
    {{
        public IConfiguration _config {{ get; }}
        public IWebHostEnvironment _env {{ get; }}

        public Startup{envName}(IConfiguration configuration, IWebHostEnvironment env)
        {{
            _config = configuration;
            _env = env;
        }}

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {{
            services.AddCorsService(""{corsName}"");
            services.AddInfrastructure(_config, _env);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddWebApiServices();
            services.AddHealthChecks();

            // Dynamic Services
        }}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {{
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            // Entity Context - Do Not Delete

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
    }}
}}";
            else

                return @$"namespace {classNamespace}
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using {apiServiceExtensionsClassPath.ClassNamespace};
    using {apiAppExtensionsClassPath.ClassNamespace};
    using Serilog;

    public class Startup{envName}
    {{
        public IConfiguration _config {{ get; }}
        public IWebHostEnvironment _env {{ get; }}

        public Startup{envName}(IConfiguration configuration, IWebHostEnvironment env)
        {{
            _config = configuration;
            _env = env;
        }}

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {{
            // TODO update CORS for your env
            services.AddCorsService(""{corsName}"");
            services.AddInfrastructure(_config, _env);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddWebApiServices();
            services.AddHealthChecks();

            // Dynamic Services
        }}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {{
            app.UseExceptionHandler(""/Error"");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();

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
    }}
}}";
        }
    }
}