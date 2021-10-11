namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerAccountViewModelsBuilder
    {
        public static void CreateViewModels(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "AccountViewModels.cs", authServerProjectName);
            var fileText = GetControllerText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetControllerText(string classNamespace)
        {
            return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace}
{{  
    public class LoggedOutViewModel
    {{
        public string PostLogoutRedirectUri {{ get; set; }}
        public string ClientName {{ get; set; }}
        public string SignOutIframeUrl {{ get; set; }}

        public bool AutomaticRedirectAfterSignOut {{ get; set; }}

        public string LogoutId {{ get; set; }}
        public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;
        public string ExternalAuthenticationScheme {{ get; set; }}
    }}

    public class LoginViewModel : LoginInputModel
    {{
        public bool AllowRememberLogin {{ get; set; }} = true;
        public bool EnableLocalLogin {{ get; set; }} = true;

        public IEnumerable<ExternalProvider> ExternalProviders {{ get; set; }} = Enumerable.Empty<ExternalProvider>();
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));

        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    }}

    public class RedirectViewModel
    {{
        public string RedirectUrl {{ get; set; }}
    }}

    public class LogoutViewModel : LogoutInputModel
    {{
        public string ShowLogoutPrompt {{ get; set; }}
    }}
}}";
        }
    }
}