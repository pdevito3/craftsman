namespace Craftsman.Builders.Bff
{
  using System;
  using System.IO.Abstractions;
  using System.Linq;
  using Enums;
  using Helpers;
  using Models;
  using static Helpers.ConstMessages;

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