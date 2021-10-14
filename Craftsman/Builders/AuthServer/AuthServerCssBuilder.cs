namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerCssBuilder
    {
        public static void CreateSiteCss(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerCssClassPath(projectDirectory, "site.css", authServerProjectName);
            var fileText = GetSiteText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static void CreateOutputCss(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerCssClassPath(projectDirectory, "output.css", authServerProjectName);
            var fileText = GetOutputText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetOutputText()
        {
            return @$"";
        }
        
        private static string GetSiteText(string authServerProjectName)
        {
            return @$"@tailwind base;
@tailwind components;
@tailwind utilities;";
        }
    }
}