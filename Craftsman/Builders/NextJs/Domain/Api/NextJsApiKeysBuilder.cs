namespace Craftsman.Builders.NextJs.Domain.Api;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsApiKeysBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsApiKeysBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateDynamicFeatureKeys(string spaDirectory, string entityName, string entityPlural)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(spaDirectory,
            entityPlural,
            NextJsFeatureCategory.Api,
            $"{FileNames.NextJsApiKeysFilename(entityName)}.ts");
        var routesIndexFileText = GetDynamicFeatureKeysText(entityName);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetDynamicFeatureKeysText(string entityName)
    {
        var keyExportName = FileNames.NextJsApiKeysExport(entityName);
        return @$"const {keyExportName} = {{
  all: [""{entityName.UppercaseFirstLetter()}s""] as const,
  lists: () => [...{keyExportName}.all, ""list""] as const,
  list: (queryParams: string) => 
    [...{keyExportName}.lists(), {{ queryParams }}] as const,
  details: () => [...{keyExportName}.all, ""detail""] as const,
  detail: (id: string) => [...{keyExportName}.details(), id] as const,
}}

export {{ {keyExportName} }};
";
    }
}
