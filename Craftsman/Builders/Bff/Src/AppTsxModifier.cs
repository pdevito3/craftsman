namespace Craftsman.Builders.Bff.Src
{
    using System;
    using System.IO;
    using Enums;
    using Helpers;

    public class DynamicFeatureRoutesModifier
    {
        public static void AddRoute(string spaDirectory, string entityName, string entityPlural)
        {
            var classPath = ClassPathHelper.BffSpaSrcClassPath(spaDirectory, "App.tsx");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                return; // silently skip this. just want to add this as a convenience if the scaffolding set up is used.

            var tempPath = $"{classPath.FullClassPath}temp";
            var listRouteName = Utilities.BffEntityListRouteComponentName(entityName);
            var haveAddedImport = false;
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains("import") && !haveAddedImport)
                        {
                            newText += @$"{Environment.NewLine}import {{ {listRouteName} }} from './features/{entityPlural}';";
                            haveAddedImport = true;
                        }
                        if (line.Contains("/* route marker"))
                        {
                            newText += @$"{Environment.NewLine}							<Route path=""/{entityPlural.LowercaseFirstLetter()}"" element={{<{listRouteName} />}} />";
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
