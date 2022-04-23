namespace Craftsman.Builders.Bff.Features.Dynamic;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DynamicFeatureBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateDynamicFeatureIndex(string spaDirectory, string entityPlural)
    {
      var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityPlural, BffFeatureCategory.Index , "index.ts");
      var fileText = GetDynamicFeatureIndexText();
      _utilities.CreateFile(classPath, fileText);
    }

    public static string GetDynamicFeatureIndexText()
    {
      return @$"export * from './routes';";
    }
}
