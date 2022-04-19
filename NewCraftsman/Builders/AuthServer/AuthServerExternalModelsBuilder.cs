namespace NewCraftsman.Builders.AuthServer
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;
    using static Helpers.ConstMessages;

    public class AuthServerExternalModelsBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AuthServerExternalModelsBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateModels(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerModelsClassPath(projectDirectory, "ExternalModels.cs", authServerProjectName);
            var fileText = GetControllerText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }
        
        public static string GetControllerText(string classNamespace)
        {
            return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace};  

public class ExternalProvider
{{
    public string DisplayName {{ get; set; }}
    public string AuthenticationScheme {{ get; set; }}
}}";
        }
    }
}