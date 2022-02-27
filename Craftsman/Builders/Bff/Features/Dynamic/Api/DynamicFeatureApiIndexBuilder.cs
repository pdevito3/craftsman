namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureApiIndexBuilder
{
	public static void CreateDynamicFeatureApiIndex(string spaDirectory, string featureName, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, featureName, BffFeatureCategory.Api , "index.ts");
		var routesIndexFileText = GetDynamicFeatureApisIndexText();
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetDynamicFeatureApisIndexText()
	{
	    return @$"export * from './recipe.keys';";
	}
}
