namespace Craftsman.Builders;

using Helpers;
using Services;

public class EditorConfigBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public EditorConfigBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateEditorConfig(string srcDirectory, string projectBaseName)
    {
        var appSettingFilename = FileNames.GetAppSettingsName();
        var classPath = ClassPathHelper.WebApiEditorConfigClassPath(srcDirectory, projectBaseName);
        var fileText = FetFileText();
        _utilities.CreateFile(classPath, fileText);
    }

    private static string FetFileText()
    {
        return @$"[*.cs]
dotnet_diagnostic.RMG012.severity = error # Unmapped or non-automappable target member for Mapperly
";
    }
}
