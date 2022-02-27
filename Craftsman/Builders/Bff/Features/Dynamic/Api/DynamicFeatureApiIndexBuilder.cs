namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureApiIndexBuilder
{
	public static void CreateDynamicFeatureApiIndex(string spaDirectory, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityPlural, BffFeatureCategory.Api , "index.ts");
		var routesIndexFileText = GetDynamicFeatureApisIndexText();
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetDynamicFeatureApisIndexText()
	{
	    return @$"export * from './recipe.keys';";
	}
}
