namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class RepositoryModifier
    {
        public static void AddInclude(string solutionDirectory, string entityName, List<EntityProperty> props)
        {
            var classPath = ClassPathHelper.RepositoryClassPath(solutionDirectory, $"{Utilities.GetRepositoryName(entityName, false)}.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            foreach(var prop in props)
            {
                if (prop.IsForeignKey)
                {
                    var tempPath = $"{classPath.FullClassPath}temp";
                    using (var input = File.OpenText(classPath.FullClassPath))
                    {
                        using (var output = new StreamWriter(tempPath))
                        {
                            string line;
                            var inlcudeText = $@"{Environment.NewLine}                .Include({prop.Name.ToLower().Substring(0, 1)} => {prop.Name.ToLower().Substring(0, 1)}.{prop.Name})";
                            bool updateNextLine = false;

                            // TODO: refactor this. it's... janky. the idea is that I have a comment to mark where 
                            // to add the include statements, but it needs to be added two lines after.
                            // this process will continually decrement the lineTracker variable, the number 
                            // doesn't matter until you get to the marker, when it will be set to 1, so we can 
                            // decrement it until we get to the line that we want to add it to
                            //
                            // with all that said, it's really awful to follow and I need to refactor this with a more elegant method
                            var lineTracker = 0;

                            while (null != (line = input.ReadLine()))
                            {
                                var newText = $"{line}";
                                if (line.Contains(@$"var collection"))
                                {
                                    newText += inlcudeText;
                                }
                                if (line.Contains(@$"include marker"))
                                {
                                    lineTracker = 2;
                                    updateNextLine = true;
                                }
                                if (updateNextLine && lineTracker == 1)
                                {
                                    newText += inlcudeText;
                                    updateNextLine = false;
                                }
                                lineTracker--;

                                output.WriteLine(newText);
                            }
                        }
                    }

                    // delete the old file and set the name of the new one to the original name
                    File.Delete(classPath.FullClassPath);
                    File.Move(tempPath, classPath.FullClassPath);

                    GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
                }
            }
        }

        private static string GetProfileText(ApiEnvironment env)
        {
            if (env.EnvironmentName == "Development")
                return $@"
    ""{env.ProfileName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""{env.EnvironmentName}""
      }},
      ""applicationUrl"": ""http://localhost:5000""
    }},";
            else
                return $@"
    ""{env.ProfileName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": false,
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""{env.EnvironmentName}""
      }}
    }},";
        }
    }
}

