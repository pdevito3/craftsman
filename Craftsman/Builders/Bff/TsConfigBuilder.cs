namespace Craftsman.Builders.Bff;

using System.IO.Abstractions;
using Helpers;
using Models;

public class TsConfigBuilder
{
    public static void CreateTsConfigPaths(string spaDirectory, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, $"tsconfig.paths.json");
        var fileText = GetTsConfigPathText();
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }
    public static void CreateTsConfig(string spaDirectory, IFileSystem fileSystem)
    {
	    var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, $"tsconfig.json");
	    var fileText = GetTsConfigText();
	    Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetTsConfigPathText()
    {
	    return @$"{{
	""compilerOptions"": {{
		""baseUrl"": ""."",
		""paths"": {{
			""@/*"": [""./src/*""]
		}}
	}}
}}
";
    }

    public static string GetTsConfigText()
    {
            return @$"{{
	""compilerOptions"": {{
		""target"": ""ESNext"",
		""lib"": [""DOM"", ""DOM.Iterable"", ""ESNext""],
		""allowJs"": false,
		""skipLibCheck"": false,
		""esModuleInterop"": false,
		""allowSyntheticDefaultImports"": true,
		""strict"": true,
		""forceConsistentCasingInFileNames"": true,
		""module"": ""ESNext"",
		""moduleResolution"": ""Node"",
		""resolveJsonModule"": true,
		""isolatedModules"": true,
		""noEmit"": true,
		""jsx"": ""react-jsx"",
		""noFallthroughCasesInSwitch"": true
	}},
	""include"": [""./src""],
	""exclude"": [""node_modules""],
	""extends"": ""./tsconfig.paths.json""
}}
";
    }
}
