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

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetApiVersioningServiceExtensionText(classPath.ClassNamespace);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static void CreateMassTransitServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"{Utilities.GetMassTransitRegistrationName()}.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetMassTransitServiceExtensionText(classPath.ClassNamespace);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static void CreateWebApiServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"WebApiServiceExtension.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetWebApiServiceExtensionText(classPath.ClassNamespace);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static void CreateCorsServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"CorsServiceExtension.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetCorsServiceExtensionText(classPath.ClassNamespace);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
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
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;

    public static class WebApiServiceExtension
    {{
        public static void AddWebApiServices(this IServiceCollection services)
        {{
            services.AddMediatR(typeof(Startup));
            services.AddMvc()
                .AddFluentValidation(cfg => {{ cfg.RegisterValidatorsFromAssemblyContaining<Startup>(); }});
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }}
    }}
}}";
        }

        public static string GetMassTransitServiceExtensionText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using MassTransit;
    using Messages;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using RabbitMQ.Client;

    public static class MassTransitServiceExtension
    {{
        public static void AddMassTransitServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {{
            if(env.EnvironmentName != ""FunctionalTesting"")
            {{
                services.AddMassTransit(mt =>
                {{
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