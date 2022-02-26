namespace Craftsman.Builders.Bff
{
  using System;
  using System.IO.Abstractions;
  using System.Linq;
  using Enums;
  using Helpers;
  using Models;
  using static Helpers.ConstMessages;

  public class ViteEnvBuilder
  {
    public static void CreateViteEnv(string spaDirectory, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaSrcClassPath(spaDirectory, "vite-env.d.ts");
      var fileText = GetViteEnvText();
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetViteEnvText()
    {
      return @$"/// <reference types=""vite/client"" />";
    }
  }
}