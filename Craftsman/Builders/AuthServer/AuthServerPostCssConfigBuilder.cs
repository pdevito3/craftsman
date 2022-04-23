namespace Craftsman.Builders.AuthServer
{
    using Helpers;
    using Services;

    public class AuthServerPostCssBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AuthServerPostCssBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreatePostCss(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerPostCssClassPath(projectDirectory, "postcss.config.js", authServerProjectName);
            var fileText = GetPostCssText();
            _utilities.CreateFile(classPath, fileText);
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