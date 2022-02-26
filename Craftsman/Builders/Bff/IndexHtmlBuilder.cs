namespace Craftsman.Builders.Bff
{
  using System;
  using System.IO.Abstractions;
  using System.Linq;
  using Enums;
  using Helpers;
  using Models;
  using static Helpers.ConstMessages;

  public class IndexHtmlBuilder
  {
    public static void CreateIndexHtml(string spaDirectory, string headTitle, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, "index.html");
      var fileText = GetIndexHtmlText(headTitle);
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetIndexHtmlText(string headTitle)
    {
      return @$"<!DOCTYPE html>
<html lang=""en"">
	<head>
		<meta charset=""UTF-8"" />
		<link rel=""icon"" type=""image/svg+xml"" href=""/src/assets/favicon.svg"" />
		<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
		<title>{headTitle}</title>
	</head>
	<body>
		<div id=""root""></div>
		<script type=""module"" src=""/src/main.tsx""></script>
	</body>
</html>
";
    }
  }
}