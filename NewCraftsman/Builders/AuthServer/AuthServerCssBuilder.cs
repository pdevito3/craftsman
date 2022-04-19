namespace NewCraftsman.Builders.AuthServer
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class AuthServerCssBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AuthServerCssBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateSiteCss(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerCssClassPath(projectDirectory, "site.css", authServerProjectName);
            var fileText = GetSiteText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }
        
        public void CreateOutputCss(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerCssClassPath(projectDirectory, "output.css", authServerProjectName);
            var fileText = GetOutputText();
            _utilities.CreateFile(classPath, fileText);
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