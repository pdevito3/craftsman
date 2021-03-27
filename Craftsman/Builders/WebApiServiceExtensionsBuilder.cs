namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class WebApiServiceExtensionsBuilder
    {
        public static void CreateWebApiServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.WebApiExtensionsClassPath(solutionDirectory, $"ServiceExtensions.cs", projectBaseName);

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetServiceExtensionText(classPath.ClassNamespace);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        public static string GetServiceExtensionText(string classNamespace)
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

    public static class ServiceExtensions
    {{
        // Swagger Marker - Do Not Delete

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
    }
}
