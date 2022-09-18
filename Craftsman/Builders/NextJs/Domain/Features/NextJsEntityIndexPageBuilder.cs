namespace Craftsman.Builders.NextJs.Domain.Features;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsEntityFeatureIndexPageBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsEntityFeatureIndexPageBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string nextSrc, string entityName, string entityPlural)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(nextSrc,
            entityPlural,
            NextJsDomainCategory.Features,
            $"index.ts");
        var routesIndexFileText = GetFileText(entityName);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName)
    {
        return @$"export * from ""./{FileNames.NextJsEntityFeatureFormName(entityName)}"";
export * from ""./{FileNames.NextJsEntityFeatureListTableName(entityName)}""; ";
    }
}
