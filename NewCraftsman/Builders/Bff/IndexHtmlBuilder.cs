namespace NewCraftsman.Builders.Bff
{
	using System.IO.Abstractions;
	using Helpers;
	using Services;

	public class IndexHtmlBuilder
  {
	  private readonly ICraftsmanUtilities _utilities;

	  public IndexHtmlBuilder(ICraftsmanUtilities utilities)
	  {
		  _utilities = utilities;
	  }

    public void CreateIndexHtml(string spaDirectory, string headTitle)
    {
      var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, "index.html");
      var fileText = GetIndexHtmlText(headTitle);
      _utilities.CreateFile(classPath, fileText);
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