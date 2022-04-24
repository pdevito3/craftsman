namespace Craftsman.Builders;

using Helpers;
using Services;

public class AppSettingsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AppSettingsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    /// <summary>
    /// this build will create environment based app settings files.
    /// </summary>
    public void CreateWebApiAppSettings(string srcDirectory, string dbName, string projectBaseName)
    {
        var appSettingFilename = FileNames.GetAppSettingsName();
        var classPath = ClassPathHelper.WebApiAppSettingsClassPath(srcDirectory, $"{appSettingFilename}", projectBaseName);
        var fileText = GetAppSettingsText(dbName);
        _utilities.CreateFile(classPath, fileText);
    }

    public void CreateAuthServerAppSettings(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.AuthServerAppSettingsClassPath(projectDirectory, $"appsettings.json", authServerProjectName);
        var fileText = @$"{{
  ""AllowedHosts"": ""*""
}}
";
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetAppSettingsText(string dbName)
    {
        // won't build properly if it has an empty string
        return @$"{{
  ""AllowedHosts"": ""*"",
}}
";
    }
}
