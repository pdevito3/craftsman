namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerHomeControllerBuilder
    {
        public static void CreateHomeController(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerControllersClassPath(projectDirectory, "HomeController.cs", authServerProjectName);
            var fileText = GetControllerText(classPath.ClassNamespace, projectDirectory, authServerProjectName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetControllerText(string classNamespace, string projectDirectory, string authServerProjectName)
        {
            var viewModelsClassPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "", authServerProjectName);
            var attrClassPath = ClassPathHelper.AuthServerAttributesClassPath(projectDirectory, "", authServerProjectName);
            
            return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using {viewModelsClassPath.ClassNamespace};
using {attrClassPath.ClassNamespace};

namespace {classNamespace};
[SecurityHeaders]
[AllowAnonymous]
public class HomeController : Controller
{{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment, ILogger<HomeController> logger)
    {{
        _interaction = interaction;
        _environment = environment;
        _logger = logger;
    }}

    public IActionResult Index()
    {{
        if (_environment.IsDevelopment())
        {{
            // only show in development
            return View();
        }}

        _logger.LogInformation(""Homepage is disabled in production. Returning 404."");
        return NotFound();
    }}

    /// <summary>
    /// Shows the error page
    /// </summary>
    public async Task<IActionResult> Error(string errorId)
    {{
        var vm = new ErrorViewModel();

        // retrieve error details from identityserver
        var message = await _interaction.GetErrorContextAsync(errorId);
        if (message != null)
        {{
            vm.Error = message;

            if (!_environment.IsDevelopment())
            {{
                // only show in development
                message.ErrorDescription = null;
            }}
        }}

        return View(""Error"", vm);
    }}
}}";
        }
    }
}