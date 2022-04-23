namespace Craftsman.Builders.ExtensionBuilders;

using Helpers;
using Services;

public class WebApiServiceExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public WebApiServiceExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }
    
    public void CreateWebApiServiceExtension(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"WebApiServiceExtension.cs", projectBaseName);
        var fileText = GetWebApiServiceExtensionText(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var middlewareClassPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, $"", projectBaseName);
        
        return @$"namespace {classNamespace};

using {servicesClassPath.ClassNamespace};
using {middlewareClassPath.ClassNamespace};
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
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        services.AddMediatR(typeof(Startup));
        services.AddScoped<SieveProcessor>();
        services.AddMvc(options => options.Filters.Add<ErrorHandlerFilterAttribute>())
            .AddFluentValidation(cfg => {{ cfg.AutomaticValidationEnabled = false; }});
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }}
}}";
    }
}
