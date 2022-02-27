namespace Craftsman.Builders.Bff.Src
{
  using System.IO.Abstractions;
  using Helpers;

  public class CustomCssBuilder
  {
    public static void CreateCustomCss(string spaDirectory, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaSrcClassPath(spaDirectory, "custom.css");
      var fileText = GetCustomCssText();
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetCustomCssText()
    {
      return @$"@tailwind base;
@tailwind components;
@tailwind utilities;";
    }
  }
}