namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class SwaggerServiceRegistrationModifier
    {
        public static void AddPolicies(string srcDirectory, List<Policy> policies, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetSwaggerServiceExtensionName()}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var policiesString = "";
            var nonExistantPolicies = Utilities.GetPoliciesThatDoNotExist(policies, classPath.FullClassPath);
            policiesString += $@"{Environment.NewLine}{Utilities.GetSwaggerPolicies(nonExistantPolicies)}";

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
                        if (line.Contains($"Scopes ="))
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
        }
    }
}

