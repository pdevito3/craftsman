namespace Craftsman.Builders.ExtensionBuilders;

using Helpers;
using MediatR;
using Services;

public static class CorsExtensionsBuilder
{
    public sealed record CorsExtensionsBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CorsExtensionsBuilderCommand>
    {
        public Task Handle(CorsExtensionsBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(scaffoldingDirectoryStore.SrcDirectory, $"CorsServiceExtension.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCorsServiceExtensionText(classPath.ClassNamespace, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetCorsServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var apiResourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, $"", projectBaseName);
        return @$"namespace {classNamespace};

using {apiResourcesClassPath.ClassNamespace};
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

public static class CorsServiceExtension
{{
    public static void AddCorsService(this IServiceCollection services, string policyName, IWebHostEnvironment env)
    {{
        if (env.IsDevelopment() || env.IsEnvironment(Consts.Testing.IntegrationTestingEnvName) ||
            env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {{
            services.AddCors(options =>
            {{
                options.AddPolicy(policyName, builder => 
                    builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders(""X-Pagination""));
            }});
        }}
        else
        {{
            //TODO update origins here with env vars or secret
            //services.AddCors(options =>
            //{{
            //    options.AddPolicy(policyName, builder =>
            //        builder.WithOrigins(origins)
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .WithExposedHeaders(""X-Pagination""));
            //}});
        }}
    }}
}}";
    }
}