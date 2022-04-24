namespace Craftsman.Builders.AuthServer;

using Helpers;
using Services;
using static Helpers.ConstMessages;

public class AuthServerAccountModelsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AuthServerAccountModelsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateModels(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.AuthServerModelsClassPath(projectDirectory, "AccountModels.cs", authServerProjectName);
        var fileText = GetControllerText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetControllerText(string classNamespace)
    {
        return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace};  

using System;
using System.ComponentModel.DataAnnotations;

public class AccountOptions
{{
    public static bool AllowLocalLogin = true;
    public static bool AllowRememberLogin = true;
    public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

    public static bool ShowLogoutPrompt = true;
    public static bool AutomaticRedirectAfterSignOut = false;

    public static string InvalidCredentialsErrorMessage = ""Invalid username or password"";
}}

public class LoginInputModel
{{
    [Required]
    public string Username {{ get; set; }}
    [Required]
    public string Password {{ get; set; }}
    public bool RememberLogin {{ get; set; }}
    public string ReturnUrl {{ get; set; }}
}}

public class LogoutInputModel
{{
    public string LogoutId {{ get; set; }}
}}";
    }
}
