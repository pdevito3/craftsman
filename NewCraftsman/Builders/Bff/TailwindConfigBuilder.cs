namespace NewCraftsman.Builders.Bff
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class TailwindConfigBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public TailwindConfigBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateTailwindConfig(string spaDirectory)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, "tailwind.config.js");
            var fileText = GetPostCssText();
            _utilities.CreateFile(classPath, fileText);
        }
        
        public static string GetPostCssText()
        {
            return @$"const colors = require('tailwindcss/colors')

module.exports = {{
	content: ['./src/**/*.{{js,jsx,ts,tsx}}', './index.html'],
	theme: {{
		extend: {{}},
	}},
  plugins: [
    require('@tailwindcss/forms'),
  ],
}}";
        }
    }
}