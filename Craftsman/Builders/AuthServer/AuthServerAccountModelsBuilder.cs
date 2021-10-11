namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerAccountModelsBuilder
    {
        public static void CreateModels(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerModelsClassPath(projectDirectory, "AccountModels.cs", authServerProjectName);
            var fileText = GetControllerText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetControllerText(string classNamespace)
        {
            return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace}
{{  
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
    }}
}}";
        }
    }
}