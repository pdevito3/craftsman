namespace Craftsman.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Spectre.Console;

    public interface IConsoleWriter
    {
        void WriteInfo(string message);
        void WriteError(string message);
        void WriteWarning(string message);
        void WriteHelpHeader(string message);
        void WriteHelpText(string message);
        void WriteLogMessage(string message);
        void StarGithubRequest();
    }

    public class ConsoleWriter : IConsoleWriter
    {
        private readonly IAnsiConsole _console;

        public ConsoleWriter(IAnsiConsole console)
        {
            _console = console;
        }
        
        public void WriteInfo(string message)
        {
            _console.MarkupLine($"[bold mediumpurple3_1]{message.EscapeMarkup()}[/]");
        }

        public void WriteError(string message)
        {
            _console.MarkupLine($"[bold indianred1]ERROR: {message.EscapeMarkup()}[/]");
        }

        public void WriteWarning(string message)
        {
            _console.MarkupLine($"[bold olive]WARNING: {message.EscapeMarkup()}[/]");
        }

        public void WriteHelpHeader(string message)
        {
            _console.MarkupLine($"[bold olive]{message.EscapeMarkup()}[/]");
        }

        public void WriteHelpText(string message)
        {
            _console.MarkupLine($"[green3]{message.EscapeMarkup()}[/]");
        }

        public void WriteLogMessage(string message)
        {
            _console.MarkupLine($"[grey]{message}.[/]");
        }

        public void StarGithubRequest()
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