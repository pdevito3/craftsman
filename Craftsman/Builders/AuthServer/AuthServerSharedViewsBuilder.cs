namespace Craftsman.Builders.AuthServer;

using Helpers;
using Services;

public class AuthServerSharedViewsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AuthServerSharedViewsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateLayoutView(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory, "_Layout.cshtml", authServerProjectName, ClassPathHelper.AuthServerViewSubDir.Shared);
        var fileText = GetLayoutText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public void CreateStartView(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.AuthServerViewsClassPath(projectDirectory, "_ViewStart.cshtml", authServerProjectName);
        var fileText = GetStartViewTest();
        _utilities.CreateFile(classPath, fileText);
    }

    public void CreateViewImports(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.AuthServerViewsClassPath(projectDirectory, "_ViewImports.cshtml", authServerProjectName);
        var fileText = GetViewImportsTest(projectDirectory, authServerProjectName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetStartViewTest()
    {
        return @$"@{{
    Layout = ""_Layout"";
}}";
    }

    private static string GetViewImportsTest(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.AuthServerControllersClassPath(projectDirectory, "", authServerProjectName);

        return @$"@using {classPath.ClassNamespace}
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
";
    }

    private static string GetLayoutText(string authServerProjectName)
    {
        return @$"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, shrink-to-fit=no"" />

    <title>{authServerProjectName}</title>
    
    <link rel=""stylesheet"" href=""~/css/output.css""/>
</head>
<body class=""bg-gray-100 bg-gradient-to-t from-gray-100 via-gray-50 to-gray-100"">
<div class=""flex flex-col h-screen w-screen overflow-hidden"">
    <div class=""flex-1 h-full w-full max-h-full"">
        @RenderBody()
    </div>
</div>

</body>
</html>";
    }
}
