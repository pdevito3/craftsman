namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerSharedViewModelsBuilder
    {
        public static void CreateViewModels(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "SharedViewModels.cs", authServerProjectName);
            var fileText = GetControllerText(classPath.ClassNamespace, projectDirectory, authServerProjectName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetControllerText(string classNamespace, string projectDirectory, string authServerProjectName)
        {
            return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace}
{{  
    using Duende.IdentityServer.Models;

    public class ErrorViewModel
    {{
        public ErrorViewModel()
        {{
        }}

        public ErrorViewModel(string error)
        {{
            Error = new ErrorMessage {{ Error = error }};
        }}

        public ErrorMessage Error {{ get; set; }}
    }}
}}";
        }
    }
}