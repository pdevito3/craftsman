namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureApiIndexBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public DynamicFeatureApiIndexBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

	public void CreateDynamicFeatureApiIndex(string spaDirectory, string entityName, string entityPlural)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityPlural, BffFeatureCategory.Api , "index.ts");
		var routesIndexFileText = GetDynamicFeatureApisIndexText(entityName);
		_utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
	}
	
	public static string GetDynamicFeatureApisIndexText(string entityName)
	{
		var keysImport = FileNames.BffApiKeysFilename(entityName);
	    return @$"export * from './{keysImport}';";
	}
}
