namespace Craftsman.Builders.Bff.Features.Home;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class HomeFeatureBuilder
{
    public static void CreateHomeFeatureIndex(string spaDirectory, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Home", BffFeatureCategory.Index , "index.ts");
      var fileText = GetHomeFeatureIndexText();
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetHomeFeatureIndexText()
    {
      return @$"export * from './routes';";
    }
}
