namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using static Helpers.ConsoleWriter;

    public class InfrastructureIdentityServiceRegistrationModifier
    {
        public static void AddPolicies(string solutionDirectory, List<Policy> policies)
        {
            var classPath = ClassPathHelper.InfrastructureIdentityProjectRootClassPath(solutionDirectory, $"ServiceRegistration.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var policiesString = "";
            foreach (var policy in policies)
            {
                policiesString += $@"{Environment.NewLine}{Utilities.PolicyStringBuilder(policy)}";
            }

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    bool updateNextLine = false;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"AddAuthorization"))
                        {
                            updateNextLine = true;
                        }
                        else if (updateNextLine)
                        {
                            newText += policiesString;
                            updateNextLine = false;
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }
    }
}

