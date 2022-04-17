namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureKeysBuilder
{
	public static void CreateDynamicFeatureKeys(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{Utilities.BffApiKeysFilename(entityName)}.ts");
		var routesIndexFileText = GetDynamicFeatureKeysText(entityName);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetDynamicFeatureKeysText(string entityName)
	{
		var keyExportName = Utilities.BffApiKeysExport(entityName);
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
