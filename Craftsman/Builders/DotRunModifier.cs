namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DotRunModifier
    {
        public static void AddRunItem(string solutionDirectory, string solutionName, string profileName, string projectBaseName)
        {
            var classPath = ClassPathHelper.DotRunClassPath(solutionDirectory, $"{solutionName}Boundaries.run.xml");

            if (!File.Exists(classPath.FullClassPath))
                return; // fail silently

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains("<configuration "))
                        {
                            newText += @$"{Environment.NewLine}    <toRun name=""{projectBaseName}: {profileName}"" type=""LaunchSettings"" />";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
