namespace Craftsman.Builders.AuthServer
{
  using System.IO.Abstractions;
  using Helpers;

    public class AuthServerTailwindConfigBuilder
    {
        public static void CreateTailwindConfig(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerTailwindConfigClassPath(projectDirectory, "tailwind.config.js", authServerProjectName);
            var fileText = GetPostCssText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetPostCssText()
        {
            return @$"const colors = require('tailwindcss/colors')

module.exports = {{
  purge: [""./**/*.cshtml"",""../**/*.cshtml"", ""../**/*.html"",""./**/*.html"", ""./**/*.razor""],
  darkMode: false, // or 'media' or 'class'
  theme: {{
    extend: {{
      colors: {{
        violet: colors.violet,
      }}
    }},
  }},
  variants: {{
    extend: {{ }},
  }},
  plugins: [
    require('@tailwindcss/forms'),
  ],
}}";
        }
    }
}