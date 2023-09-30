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
        var fileText = GetAppSettingsText();
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetAppSettingsText()
    {
        // won't build properly if it has an empty string
        return @$"{{
  ""AllowedHosts"": ""*"",
  ""Serilog"": {{
    ""MinimumLevel"": {{
      ""Default"": ""Information"",
      ""Override"": {{
        ""Microsoft.Hosting.Lifetime"": ""Information"",
        ""Microsoft.AspNetCore.Authentication"": ""Information""
      }}
    }},
    ""Enrich"": [
      ""FromLogContext"", 
      ""WithExceptionDetails"",
      ""WithMachineName"",
      ""WithThreadId""
    ],
    ""WriteTo"": [
      {{
        ""Name"": ""Console""
      }}
    ]
  }}
}}
";
    }
}
