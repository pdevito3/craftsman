namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;

    public class WebApiAppSettingsModifier
    {
        public static void AddRmq(string solutionDirectory, ApiEnvironment env, string projectBaseName, IFileSystem fileSystem)
        {
            var appSettingFilename = Utilities.GetAppSettingsName(env.EnvironmentName);
            var classPath = ClassPathHelper.WebApiAppSettingsClassPath(solutionDirectory, $"{appSettingFilename}", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            List<string> lines = File.ReadAllLines(classPath.FullClassPath).ToList();
            lines[^2] = lines[^2].Replace(",", "") + GetRmqText(env); // lines[^2] == lines[lines.Count - 2]
            File.WriteAllLines(classPath.FullClassPath, lines);
        }

        private static string GetRmqText(ApiEnvironment env)
        {
            return $@",
  ""RMQ"": {{
    ""Host"": ""{env.BrokerSettings.Host}"",
    ""VirtualHost"": ""{env.BrokerSettings.VirtualHost}"",
    ""Username"": ""{env.BrokerSettings.Username}"",
    ""Password"": ""{env.BrokerSettings.Password}""
  }}";
        }
    }
}