namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class WebApiServiceExtensionsBuilder
    {
        public static void CreateApiVersioningServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"ApiVersioningServiceExtension.cs", projectBaseName);
            var fileText = GetApiVersioningServiceExtensionText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateMassTransitServiceExtension(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetMassTransitRegistrationName()}.cs", projectBaseName);
            var fileText = GetMassTransitServiceExtensionText(classPath.ClassNamespace, srcDirectory, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateWebApiServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"WebApiServiceExtension.cs", projectBaseName);
            var fileText = GetWebApiServiceExtensionText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateCorsServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"CorsServiceExtension.cs", projectBaseName);
            var fileText = GetCorsServiceExtensionText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetApiVersioningServiceExtensionText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ApiVersioningServiceExtension
    {{
        public static void AddApiVersioningExtension(this IServiceCollection services)
        {{
            services.AddApiVersioning(config =>
            {{
                // Default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // use default version when version is not specified
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            }});
        }}
    }}
}}";
        }

        public static string GetCorsServiceExtensionText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;

    public static class CorsServiceExtension
    {{
        public static void AddCorsService(this IServiceCollection services, string policyName)
        {{
            services.AddCors(options =>
            {{
                options.AddPolicy(policyName,
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(""X-Pagination""));
            }});
        }}
    }}
}}";
        }

        public static string GetWebApiServiceExtensionText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Sieve.Services;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;

    public static class WebApiServiceExtension
    {{
        public static void AddWebApiServices(this IServiceCollection services)
        {{
            services.AddMediatR(typeof(Startup));
            services.AddScoped<SieveProcessor>();
            services.AddMvc()
                .AddFluentValidation(cfg => {{ cfg.RegisterValidatorsFromAssemblyContaining<Startup>(); }});
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }}
    }}
}}";
        }

        public static string GetMassTransitServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName)
        {
            var utilsClassPath = ClassPathHelper.WebApiUtilsClassPath(srcDirectory, "", projectBaseName);
            
            return @$"namespace {classNamespace}
{{
    using {utilsClassPath.ClassNamespace};
    using MassTransit;
    using Messages;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using RabbitMQ.Client;
    using System.Reflection;

    public static class MassTransitServiceExtension
    {{
        public static void AddMassTransitServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {{
            if (!env.IsEnvironment(LocalConfig.IntegrationTestingEnvName) 
                && !env.IsEnvironment(LocalConfig.FunctionalTestingEnvName) 
                && !env.IsDevelopment())
            {{
                services.AddMassTransit(mt =>
                {{
                    mt.AddConsumers(Assembly.GetExecutingAssembly());
                    mt.UsingRabbitMq((context, cfg) =>
                    {{
                        cfg.Host(configuration[""RMQ:Host""], configuration[""RMQ:VirtualHost""], h =>
                        {{
                            h.Username(configuration[""RMQ:Username""]);
                            h.Password(configuration[""RMQ:Password""]);
                        }});

                        // Producers -- Do Not Delete This Comment

                        // Consumers -- Do Not Delete This Comment
                    }});
                }});
                services.AddMassTransitHostedService();
            }}
        }}
    }}
}}";
        }
    }
}