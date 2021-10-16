namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerSharedViewsBuilder
    {
        public static void CreateLayoutView(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory, "_Layout.cshtml", authServerProjectName, ClassPathHelper.AuthServerViewSubDir.Shared);
            var fileText = GetLayoutText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static void CreateStartView(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerViewsClassPath(projectDirectory, "_ViewStart.cshtml", authServerProjectName);
            var fileText = GetStartViewTest();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetStartViewTest()
        {
            return @$"@{{
    Layout = ""_Layout"";
}}";
        }
        
        private static string GetLayoutText(string authServerProjectName)
        {
            return @$"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, shrink-to-fit=no"" />

    <title>{authServerProjectName}</title>
    
    <link rel=""stylesheet"" href=""~/css/output.css""/>
</head>
<body class=""bg-gray-100 bg-gradient-to-t from-gray-100 via-gray-50 to-gray-100"">
<div class=""flex flex-col h-screen w-screen overflow-hidden"">
    <div class=""flex-1 h-full w-full max-h-full"">
        @RenderBody()
    </div>
</div>

</body>
</html>";
        }
    }
}