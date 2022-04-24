namespace Craftsman.Builders.Bff.Src;

using Helpers;
using Services;

public class ViteEnvBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ViteEnvBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateViteEnv(string spaDirectory)
    {
        var classPath = ClassPathHelper.BffSpaSrcClassPath(spaDirectory, "vite-env.d.ts");
        var fileText = GetViteEnvText();
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetViteEnvText()
    {
        return @$"/// <reference types=""vite/client"" />";
    }
}
