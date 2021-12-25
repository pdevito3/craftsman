namespace Craftsman.Builders.Auth
{
    using System;
    using System.IO;
    using Helpers;

    public class PermissionsModifier
    {
        public static void AddPermission(string srcDirectory, string permission, string projectBaseName)
        {
            var classPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, $"Permissions.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var fileText = File.ReadAllText(classPath.FullClassPath);
            if (fileText.Contains($"const string {permission}"))
                return;

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"Permissions marker"))
                        {
                            newText += @$"{Environment.NewLine}    public const string {permission} = ""{permission}"";";
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
