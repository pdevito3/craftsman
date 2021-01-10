namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class StartupBuilder
    {
        public static void CreateStartup(string solutionDirectory, string envName, ApiTemplate template)
        {
            try
            {
                var classPath = envName == "Startup" ? ClassPathHelper.StartupClassPath(solutionDirectory, $"Startup.cs") : ClassPathHelper.StartupClassPath(solutionDirectory, $"Startup{envName}.cs");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetStartupText(envName, template);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetStartupText(string envName, ApiTemplate template)
        {
            var authServices = "";
            var authApp = "";
            var authSeeder = "";
            var authUsing = "";
            var currentUserRegistration = "";
            if (template.AuthSetup.AuthMethod == "JWT")
            {
                authServices = @"
            services.AddIdentityInfrastructure(_config);";
                authApp = @"app.UseAuthentication();
            app.UseAuthorization();";
                var userSeeders = "";
                if(template.AuthSetup.InMemoryUsers != null)
                {
                    foreach(var user in template.AuthSetup.InMemoryUsers)
                    {
                        var newLine = user == template.AuthSetup.InMemoryUsers.LastOrDefault() ? "" : $"{Environment.NewLine}           ";
                        var seederName = Utilities.GetIdentitySeederName(user);
                        userSeeders += @$"{seederName}.SeedUserAsync(userManager);{newLine}";
                    }
                }
                authSeeder = $@"

            #region Identity Context Region - Do Not Delete

            var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
            var roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();
            RoleSeeder.SeedDemoRolesAsync(roleManager);

            // user seeders -- do not delete this comment
            {userSeeders}

            #endregion";

                authUsing = @$"
    using Infrastructure.Identity;
    using Infrastructure.Identity.Entities;
    using Microsoft.AspNetCore.Identity;
    using Infrastructure.Identity.Seeders;
    using WebApi.Services;
    using Application.Interfaces;";

                currentUserRegistration = $@"
            services.AddSingleton<ICurrentUserService, CurrentUserService>();";
            }

            envName = envName == "Startup" ? "" : envName;
            if (envName == "Development")

                return @$"namespace WebApi
{{
    using Application;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Persistence;
    using Infrastructure.Shared;
    using Infrastructure.Persistence.Seeders;
    using Infrastructure.Persistence.Contexts;
    using WebApi.Extensions;
    using Serilog;{authUsing}

    public class Startup{envName}
    {{
        public IConfiguration _config {{ get; }}
        public Startup{envName}(IConfiguration configuration)
        {{
            _config = configuration;
        }}

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {{
            services.AddCorsService(""MyCorsPolicy"");
            services.AddApplicationLayer();{authServices}
            services.AddPersistenceInfrastructure(_config);
            services.AddSharedInfrastructure(_config);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddHealthChecks();{currentUserRegistration}

            #region Dynamic Services
            #endregion
        }}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {{
            app.UseDeveloperExceptionPage();

            #region Entity Context Region - Do Not Delete
            #endregion{authSeeder}

            app.UseCors(""MyCorsPolicy"");

            app.UseSerilogRequestLogging();
            app.UseRouting();
            {authApp}
            app.UseErrorHandlingMiddleware();
            app.UseEndpoints(endpoints =>
            {{
                endpoints.MapHealthChecks(""/api/health"");
                endpoints.MapControllers();
            }});

            #region Dynamic App
            #endregion
        }}
    }}
}}";
        else

            return @$"namespace WebApi
{{
    using Application;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Persistence;
    using Infrastructure.Shared;
    using WebApi.Extensions;
    using Serilog;{authUsing}

    public class Startup{envName}
    {{
        public IConfiguration _config {{ get; }}
        public Startup{envName}(IConfiguration configuration)
        {{
            _config = configuration;
        }}

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {{
            services.AddCorsService(""MyCorsPolicy"");
            services.AddApplicationLayer();{authServices}
            services.AddPersistenceInfrastructure(_config);
            services.AddSharedInfrastructure(_config);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddHealthChecks();{currentUserRegistration}

            #region Dynamic Services
            #endregion
        }}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {{
            app.UseCors(""MyCorsPolicy"");

            app.UseSerilogRequestLogging();
            app.UseRouting();
            {authApp}
            app.UseErrorHandlingMiddleware();
            app.UseEndpoints(endpoints =>
            {{
                endpoints.MapHealthChecks(""/api/health"");
                endpoints.MapControllers();
            }});

            #region Dynamic App
            #endregion
        }}
    }}
}}";
        }
    }
}
