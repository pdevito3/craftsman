namespace NewCraftsman.Interceptors;

using System.Runtime.InteropServices;
using System.Text;
using Spectre.Console.Cli;

public class OperatingSystemInterceptor : ICommandInterceptor
{
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // this makes emojis come up more reliably. might get built into spectre better in the future, so give a go deleting this at some point
            // they seem to show up fine on osx and actually need this to be off to work there
            Console.OutputEncoding = Encoding.Unicode;
        }
    }
}