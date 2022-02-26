namespace Craftsman.Builders.Bff
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class PrettierRcBuilder
    {
        public static void CreatePrettierRc(string spaDirectory, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, ".prettierrc");
            var fileText = GetPrettierRcText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetPrettierRcText()
        {
            return @$"{{
  ""singleQuote"": true,
  ""trailingComma"": ""es5"",
  ""printWidth"": 100,
  ""tabWidth"": 2,
  ""useTabs"": true
}}";
        }
    }
}