namespace Craftsman.Builders.NextJs.Domain;

using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsEntityIndexBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsEntityIndexBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string nextSrc, string entityName, string entityPlural, List<NextJsEntityProperty> properties)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(nextSrc,
            entityPlural,
            NextJsDomainCategory.Index,
            $"index.ts");
        var routesIndexFileText = GetFileText(entityName, properties);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, List<NextJsEntityProperty> properties)
    {
        var validationSchema = FileNames.NextJsEntityValidationName(entityName);

        return @$"export * from ""./api"";
export * from ""./features"";
export * from ""./types"";
export * from ""./validation"";
";
    }
}
