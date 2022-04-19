namespace NewCraftsman.Builders.AuthServer
{
  using System.IO.Abstractions;
  using Helpers;
  using Services;

  public class AuthServerTailwindConfigBuilder
    {
      private readonly ICraftsmanUtilities _utilities;

      public AuthServerTailwindConfigBuilder(ICraftsmanUtilities utilities)
      {
        _utilities = utilities;
      }

        public void CreateTailwindConfig(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerTailwindConfigClassPath(projectDirectory, "tailwind.config.js", authServerProjectName);
            var fileText = GetPostCssText();
            _utilities.CreateFile(classPath, fileText);
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