namespace Craftsman.Builders.Bff
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class EnvBuilder
    {
        public static void CreateDevEnv(string spaDirectory, int? spaPort, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, ".env.development");
            var fileText = GetEnvDevText(spaPort);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        public static void CreateEnv(string spaDirectory, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, ".env");
            var fileText = GetEnvText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
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