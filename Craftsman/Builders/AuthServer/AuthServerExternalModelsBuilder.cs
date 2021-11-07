namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerExternalModelsBuilder
    {
        public static void CreateModels(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerModelsClassPath(projectDirectory, "ExternalModels.cs", authServerProjectName);
            var fileText = GetControllerText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
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