namespace NewCraftsman.Builders.Bff;

using System.IO.Abstractions;
using Helpers;
using Services;

public class TsConfigBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public TsConfigBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

    public void CreateTsConfigPaths(string spaDirectory)
    {
        var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, $"tsconfig.paths.json");
        var fileText = GetTsConfigPathText();
        _utilities.CreateFile(classPath, fileText);
    }
    public void CreateTsConfig(string spaDirectory)
    {
	    var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, $"tsconfig.json");
	    var fileText = GetTsConfigText();
	    _utilities.CreateFile(classPath, fileText);
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
