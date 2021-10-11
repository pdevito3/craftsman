namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerPostCssBuilder
    {
        public static void CreatePostCss(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerPostCssClassPath(projectDirectory, "postcss.config.js", authServerProjectName);
            var fileText = GetPostCssText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetPostCssText()
        {
            return @$"module.exports = {{
  plugins: [
    require(""tailwindcss"")(""./tailwind.config.js""),
    require(""autoprefixer""),
  ],
}};";
        }
    }
}