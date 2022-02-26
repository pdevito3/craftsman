namespace Craftsman.Builders.Bff
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class TailwindConfigBuilder
    {
        public static void CreateTailwindConfig(string spaDirectory, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, "tailwind.config.js");
            var fileText = GetPostCssText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
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