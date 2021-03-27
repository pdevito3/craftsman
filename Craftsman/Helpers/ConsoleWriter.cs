namespace Craftsman.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ConsoleWriter
    {
        public static void WriteInfo(string message)
        {
            var origBgColor = Console.BackgroundColor;
            var origTextColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);

            Console.BackgroundColor = origBgColor;
            Console.ForegroundColor = origTextColor;
        }

        public static void WriteError(string message)
        {
            var origBgColor = Console.BackgroundColor;
            var origTextColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);

            Console.BackgroundColor = origBgColor;
            Console.ForegroundColor = origTextColor;
        }

        public static void WriteWarning(string message)
        {
            var origBgColor = Console.BackgroundColor;
            var origTextColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.WriteLine(message);

            Console.BackgroundColor = origBgColor;
            Console.ForegroundColor = origTextColor;
        }

        public static void WriteHelpHeader(string message)
        {
            var origBgColor = Console.BackgroundColor;
            var origTextColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.WriteLine(message);

            Console.BackgroundColor = origBgColor;
            Console.ForegroundColor = origTextColor;
        }

        public static void WriteHelpText(string message)
        {
            var origBgColor = Console.BackgroundColor;
            var origTextColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(message);

            Console.BackgroundColor = origBgColor;
            Console.ForegroundColor = origTextColor;
        }
        
        public static void WriteGettingStarted(string solutionName)
        {
            WriteHelpText(@$"{Environment.NewLine}
    To get started:");
            WriteHelpText(@$"
        cd {solutionName}
        dotnet run --project webapi{Environment.NewLine}");
        }

        public static void StarGithubRequest()
        {
            WriteHelpText(@$"{Environment.NewLine}Would you like to show some love by starring the repo? (y/n) [n]");
            var starRepo = Console.ReadKey();
            if (starRepo.Key == ConsoleKey.Y)
            {
                WriteHelpText($"{Environment.NewLine}Thanks, it means the world to me!");
                var url = "https://github.com/pdevito3/craftsman";
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", url);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", url);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else {
                WriteHelpText($"{Environment.NewLine}I understand, but am not going to pretend I'm not sad about it...");
            }
        }
    }
}
