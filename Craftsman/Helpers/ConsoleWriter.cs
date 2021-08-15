namespace Craftsman.Helpers
{
    using Spectre.Console;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class ConsoleWriter
    {
        public static void WriteInfo(string message)
        {
            AnsiConsole.MarkupLine($"[bold mediumpurple3_1]{message.EscapeMarkup()}[/]");
        }

        public static void WriteError(string message)
        {
            AnsiConsole.MarkupLine($"[bold indianred1]ERROR: {message.EscapeMarkup()}[/]");
        }

        public static void WriteWarning(string message)
        {
            AnsiConsole.MarkupLine($"[bold olive]WARNING: {message.EscapeMarkup()}[/]");
        }

        public static void WriteHelpHeader(string message)
        {
            AnsiConsole.MarkupLine($"[bold olive]{message.EscapeMarkup()}[/]");
        }

        public static void WriteHelpText(string message)
        {
            AnsiConsole.MarkupLine($"[green3]{message.EscapeMarkup()}[/]");
        }

        public static void WriteLogMessage(string message)
        {
            AnsiConsole.MarkupLine($"[grey]{message}.[/]");
        }

        public static void WriteGettingStarted(string projectName)
        {
            WriteHelpText(@$"{Environment.NewLine}
    To get started:");
            WriteHelpText(@$"
        cd {projectName}
        dotnet run --project webapi{Environment.NewLine}");
        }

        public static void StarGithubRequest()
        {
            WriteHelpText(@$"{Environment.NewLine}Would you like to show some love by starring the repo? {Emoji.Known.Star} (y/n) [n]");
            var starRepo = Console.ReadKey();
            if (starRepo.Key == ConsoleKey.Y)
            {
                WriteHelpText($"{Environment.NewLine}Thanks, it means the world to me! {Emoji.Known.PartyingFace}");
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
            else
            {
                WriteHelpText($"{Environment.NewLine}I understand, but am not going to pretend I'm not sad about it...");
            }
        }
    }
}