namespace Craftsman.Builders.Bff.Features.Dynamic;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureBuilder
{
    public static void CreateDynamicFeatureIndex(string spaDirectory, string entityPlural, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityPlural, BffFeatureCategory.Index , "index.ts");
      var fileText = GetDynamicFeatureIndexText();
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetDynamicFeatureIndexText()
    {
      return @$"export * from './routes';";
    }
}
