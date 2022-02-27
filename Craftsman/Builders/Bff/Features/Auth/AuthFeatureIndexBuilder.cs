namespace Craftsman.Builders.Bff.Features.Auth;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class AuthFeatureBuilder
{
    public static void CreateAuthFeatureIndex(string spaDirectory, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Auth", BffFeatureCategory.Index , "index.ts");
      var fileText = GetAuthFeatureIndexText();
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetAuthFeatureIndexText()
    {
      return @$"export * from './api/useAuthUser';
export * from './routes';";
    }
}
