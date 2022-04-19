namespace NewCraftsman.Builders.AuthServer
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;
    using static Helpers.ConstMessages;

    public class AuthServerSharedViewModelsBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AuthServerSharedViewModelsBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateViewModels(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "SharedViewModels.cs", authServerProjectName);
            var fileText = GetControllerText(classPath.ClassNamespace, projectDirectory, authServerProjectName);
            _utilities.CreateFile(classPath, fileText);
        }
        
        private static string GetControllerText(string classNamespace, string projectDirectory, string authServerProjectName)
        {
            return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace};

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
}}";
        }
    }
}