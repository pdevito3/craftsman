namespace Craftsman.Builders.Bff
{
    using Helpers;
    using Services;

    public class EnvBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public EnvBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateDevEnv(string spaDirectory, int? spaPort)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, ".env.development");
            var fileText = GetEnvDevText(spaPort);
            _utilities.CreateFile(classPath, fileText);
        }
        public void CreateEnv(string spaDirectory)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, ".env");
            var fileText = GetEnvText();
            _utilities.CreateFile(classPath, fileText);
        }
        
        public static string GetEnvDevText(int? spaPort)
        {
            return @$"PORT={spaPort}
HTTPS=true;";
        }
        
        public static string GetEnvText()
        {
            return @$"BROWSER=none";
        }
    }
}