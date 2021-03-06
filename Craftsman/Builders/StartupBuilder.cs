namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class StartupBuilder
    {
        public static void CreateWebApiStartup(string solutionDirectory, string envName, bool useJwtAuth, string projectBaseName = "")
        {
            try
            {
                var classPath = envName == "Startup" 
                    ? ClassPathHelper.StartupClassPath(solutionDirectory, $"Startup.cs", projectBaseName) 
                    : ClassPathHelper.StartupClassPath(solutionDirectory, $"Startup{envName}.cs", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetStartupText(solutionDirectory, classPath.ClassNamespace, envName, useJwtAuth, projectBaseName);
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

        public static string GetStartupText(string solutionDirectory, string classNamespace, string envName, bool useJwtAuth, string projectBaseName = "")
        {
            var appAuth = "";
            var identityUsing = "";
            var apiExtensionsClassPath = ClassPathHelper.WebApiExtensionsClassPath(solutionDirectory, "", projectBaseName);
            var identityServiceRegistration = "";
            if (useJwtAuth)
            {
                identityServiceRegistration = $@"
            services.AddIdentityInfrastructure(_config, _env);";
                identityUsing = $@"
    using Infrastructure.Identity;";
                appAuth = $@"

            app.UseAuthentication();
            app.UseAuthorization();";
            }

            envName = envName == "Startup" ? "" : envName;
            if (envName == "Development")
                return @$"namespace {classNamespace}
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
    using {apiExtensionsClassPath.ClassNamespace};
    using Serilog;{identityUsing}

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
            services.AddCorsService(""MyCorsPolicy"");
            services.AddApplicationLayer();
            services.AddPersistenceInfrastructure(_config);{identityServiceRegistration}
            services.AddSharedInfrastructure(_config);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddHealthChecks();

            #region Dynamic Services
            #endregion
        }}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {{
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            #region Entity Context Region - Do Not Delete
            #endregion

            app.UseCors(""MyCorsPolicy"");

            app.UseSerilogRequestLogging();
            app.UseRouting();{appAuth}
            
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

            return @$"namespace {classNamespace}
{{
    using Application;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Persistence;
    using Infrastructure.Shared;
    using {apiExtensionsClassPath.ClassNamespace};
    using Serilog;{identityUsing}

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
            services.AddCorsService(""MyCorsPolicy"");
            services.AddApplicationLayer();
            services.AddPersistenceInfrastructure(_config);{identityServiceRegistration}
            services.AddSharedInfrastructure(_config);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddHealthChecks();

            #region Dynamic Services
            #endregion
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
            
            app.UseCors(""MyCorsPolicy"");

            app.UseSerilogRequestLogging();
            app.UseRouting();{appAuth}
            
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


        public static string GetStartupText(string classNamespace, string envName)
        {
            envName = envName == "Startup" ? "" : envName;
            if (envName == "Development")

                return @$"namespace {classNamespace}
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Ocelot.DependencyInjection;
    using Ocelot.Middleware;

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
            services.AddHttpClient();

            services.AddOcelot();
        }}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {{
            app.UseDeveloperExceptionPage();
            app.UseSerilogRequestLogging();

            await app.UseOcelot();   
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
    using Serilog;
    using Ocelot.DependencyInjection;
    using Ocelot.Middleware;

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
            services.AddHttpClient();

            services.AddOcelot();
        }}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {{
            app.UseSerilogRequestLogging();
            await app.UseOcelot(); 
        }}
    }}
}}";
        }
    }
}
