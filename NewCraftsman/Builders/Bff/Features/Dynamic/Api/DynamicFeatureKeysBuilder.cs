namespace NewCraftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureKeysBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public DynamicFeatureKeysBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

	public void CreateDynamicFeatureKeys(string spaDirectory, string entityName, string entityPlural)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FileNames.BffApiKeysFilename(entityName)}.ts");
		var routesIndexFileText = GetDynamicFeatureKeysText(entityName);
		_utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
	}
	
	public static string GetDynamicFeatureKeysText(string entityName)
	{
		var keyExportName = FileNames.BffApiKeysExport(entityName);
	    return @$"const {keyExportName} = {{
  all: ['{entityName.UppercaseFirstLetter()}s'] as const,
  lists: () => [...{keyExportName}.all, 'list'] as const,
  list: (queryParams: string) => [...{keyExportName}.lists(), {{ queryParams }}] as const,
  details: () => [...{keyExportName}.all, 'detail'] as const,
  detail: (id: string) => [...{keyExportName}.details(), id] as const,
}}

export {{ {keyExportName} }};
";
	}
}
