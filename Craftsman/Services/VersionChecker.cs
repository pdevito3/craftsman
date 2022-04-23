namespace Craftsman.Services;

using System.Reflection;
using RestSharp;
using Spectre.Console;

public static class VersionChecker
{
    public static void CheckForLatestVersion()
    {
        try
        {
            var installedVersion = GetInstalledCraftsmanVersion();

            // not sure if/how to account for prerelease yet -- seems like with other repos, the logic github currently uses will redirect to the latest current
            //var isPrelease = installedVersion.IndexOf("-", StringComparison.Ordinal) > -1;

            //if (!isPrelease)
            //{
            var client = new RestClient("https://github.com/pdevito3/craftsman/releases/latest");
            var request = new RestRequest() { Method = Method.GET };
            request.AddHeader("Accept", "text/html");
            var todos = client.Execute(request);

            var latestVersion = todos.ResponseUri.Segments.LastOrDefault();
            if (latestVersion.FirstOrDefault() == 'v')
                latestVersion = latestVersion[1..]; // remove the 'v' prefix. equivalent to `latest.Substring(1, latest.Length - 1)`
         
            if (installedVersion != latestVersion)
                AnsiConsole.MarkupLine(@$"{Environment.NewLine}[bold seagreen2]This Craftsman version '{installedVersion}' is older than that of the runtime '{latestVersion}'. Update the tools for the latest features and bug fixes (`dotnet tool update -g craftsman`).[/]{Environment.NewLine}");
            //}
        }
        catch (Exception)
        {
            // fail silently
        }
    }

    public static string GetInstalledCraftsmanVersion()
    {
        var installedVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        installedVersion = installedVersion[0..^2]; // equivalent to installedVersion.Substring(0, installedVersion.Length - 2);

        return installedVersion;
    }
}