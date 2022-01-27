namespace Craftsman.Tests.FileTextTests
{
    using Craftsman.Builders;
    using FluentAssertions;
    using Xunit;

    public class StartupFileTextTests
    {
        [Fact]
        public void GetStartupText_Development_env_returns_expected_text()
        {
            var fileText = StartupBuilder.GetWebApiStartupText("", "WebApi", false, "MyBc");

            var expectedText = @$"namespace WebApi
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using MyBc.Infrastructure;
    using MyBc.Infrastructure.Seeders;
    using MyBc.Infrastructure.Contexts;
    using MyBc.WebApi.Extensions;
    using Serilog;

    public class StartupDevelopment
    {{
        public IConfiguration _config {{ get; }}
        public IWebHostEnvironment _env {{ get; }}

        public StartupDevelopment(IConfiguration configuration, IWebHostEnvironment env)
        {{
            _config = configuration;
            _env = env;
        }}

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {{
            services.AddCorsService(""MyBcCorsPolicy"");
            services.AddInfrastructure(_config, _env);
            services.AddControllers()
                .AddJsonOptions();
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

            #region Entity Context Region - Do Not Delete
            #endregion

            app.UseCors(""MyBcCorsPolicy"");

            app.UseSerilogRequestLogging();
            app.UseRouting();
            
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

            fileText.Should().Be(expectedText);
        }

        [Theory]
        [InlineData("Production")]
        [InlineData("Qa")]
        [InlineData("Staging")]
        [InlineData("Local")]
        public void GetStartupText_NonDevlopment_env_returns_expected_text(string env)
        {
            var fileText = StartupBuilder.GetWebApiStartupText("", "WebApi", false, "MyBc");
            var suffix = env == "Production" ? "" : env;

            var expectedText = @$"namespace WebApi
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using MyBc.Infrastructure;
    using MyBc.WebApi.Extensions;
    using Serilog;

    public class Startup{suffix}
    {{
        public IConfiguration _config {{ get; }}
        public IWebHostEnvironment _env {{ get; }}

        public Startup{suffix}(IConfiguration configuration, IWebHostEnvironment env)
        {{
            _config = configuration;
            _env = env;
        }}

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {{
            services.AddCorsService(""MyBcCorsPolicy"");
            services.AddInfrastructure(_config, _env);
            services.AddControllers()
                .AddJsonOptions();
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
            
            app.UseCors(""MyBcCorsPolicy"");

            app.UseSerilogRequestLogging();
            app.UseRouting();
            
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

            fileText.Should().Be(expectedText);
        }
    }
}
