namespace Craftsman.Builders.Bff
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class PostCssBuilder
    {
        public static void CreatePostCss(string projectDirectory, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(projectDirectory, "postcss.config.js");
            var fileText = GetPostCssText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetPostCssText()
        {
            return @$"module.exports = {{
	plugins: {{
		tailwindcss: {{}},
		autoprefixer: {{}},
	}},
}};";
        }
    }
}