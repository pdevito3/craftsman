namespace NewCraftsman.Builders.AuthServer
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class AuthServerPackageJsonBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AuthServerPackageJsonBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreatePackageJson(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerPackageJsonClassPath(projectDirectory, "package.json", authServerProjectName);
            var fileText = GetPackageJsonText();
            _utilities.CreateFile(classPath, fileText);
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
    ""@tailwindcss/forms"": ""^0.3.4"",
    ""tailwindcss"": ""^2.2.16""
  }},
  ""author"": ""Built with Craftsman - https://github.com/pdevito3/craftsman""
}}";
        }
    }
}