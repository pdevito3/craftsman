namespace NewCraftsman.Builders
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Domain;
    using Services;

    public class WebApiAppSettingsModifier
    {
        private readonly IFileSystem _fileSystem;

        public WebApiAppSettingsModifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddRmq(string solutionDirectory, ApiEnvironment env, string projectBaseName)
        {
            var appSettingFilename = FileNames.GetAppSettingsName();
            var classPath = ClassPathHelper.WebApiAppSettingsClassPath(solutionDirectory, $"{appSettingFilename}", projectBaseName);

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!_fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            List<string> lines = _fileSystem.File.ReadAllLines(classPath.FullClassPath).ToList();
            lines[^2] = lines[^2].Replace(",", "") + GetRmqText(env); // lines[^2] == lines[lines.Count - 2]
            _fileSystem.File.WriteAllLines(classPath.FullClassPath, lines);
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