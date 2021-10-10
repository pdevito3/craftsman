namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerPackageJsonBuilder
    {
        public static void CreatePackageJson(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerPackageJsonClassPath(projectDirectory, "package.json", authServerProjectName);
            var fileText = GetPackageJsonText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetPackageJsonText()
        {
            return @$"{{
  ""dependencies"": {{ }},
  ""scripts"": {{
    ""css:build"": ""npx tailwind build ./wwwroot/css/site.css -o ./wwwroot/css/output.css""
  }},
  ""devDependencies"": {{
    ""autoprefixer"": ""^10.3.4"",
    ""postcss"": ""^8.3.6"",
    ""tailwindcss"": ""^2.2.16""
  }},
  ""author"": ""Built with Craftsman - https://github.com/pdevito3/craftsman""
}}";
        }
    }
}