namespace Craftsman.Builders.NextJs.Domain;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class DomainIndexBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DomainIndexBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateDynamicFeatureIndex(string spaDirectory, string entityPlural)
    {
        var classPath = ClassPathHelper.NextJsSpaFeatureClassPath(spaDirectory, entityPlural, NextJsDomainCategory.Index, "index.ts");
        var fileText = GetDynamicFeatureIndexText();
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetDynamicFeatureIndexText()
    {
        return @$"export * from ""./api"";
export * from ""./features"";
export * from ""./types"";
export * from ""./validation"";";
    }
}
