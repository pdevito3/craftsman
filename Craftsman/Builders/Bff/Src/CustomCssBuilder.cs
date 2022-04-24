namespace Craftsman.Builders.Bff.Src;

using Helpers;
using Services;

public class CustomCssBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CustomCssBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCustomCss(string spaDirectory)
    {
        var classPath = ClassPathHelper.BffSpaSrcClassPath(spaDirectory, "custom.css");
        var fileText = GetCustomCssText();
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCustomCssText()
    {
        return @$"@tailwind base;
@tailwind components;
@tailwind utilities;";
    }
}
