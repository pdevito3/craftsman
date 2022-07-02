namespace Craftsman.Builders.ExtensionBuilders;

using Helpers;
using Services;

public class CorsExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CorsExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCorsServiceExtension(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"CorsServiceExtension.cs", projectBaseName);
        var fileText = GetCorsServiceExtensionText(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCorsServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var apiResourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, $"", projectBaseName);
        return @$"namespace {classNamespace};

using {apiResourcesClassPath.ClassNamespace};
using AutoMapper;
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