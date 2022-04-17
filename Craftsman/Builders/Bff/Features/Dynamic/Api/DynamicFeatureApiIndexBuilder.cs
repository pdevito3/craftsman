namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureApiIndexBuilder
{
	public static void CreateDynamicFeatureApiIndex(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityPlural, BffFeatureCategory.Api , "index.ts");
		var routesIndexFileText = GetDynamicFeatureApisIndexText(entityName);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetDynamicFeatureApisIndexText(string entityName)
	{
		var keysImport = Utilities.BffApiKeysFilename(entityName);
	    return @$"export * from './{keysImport}';";
	}
}
