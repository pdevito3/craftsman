namespace Craftsman.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public static void WriteFileCreatedUpdatedResponse()
        {
            var created = GlobalSingleton.GetCreatedFiles();
            var updated = GlobalSingleton.GetUpdatedFiles();

            created = created.OrderBy(i => i).Distinct().ToList();
            updated = updated.OrderBy(i => i).Distinct().ToList();

            WriteHelpHeader($"Files Created:");
            foreach(var file in created)
            {
                WriteHelpText($"    {file}");
            }

            WriteHelpHeader($"Files Updated:");
            foreach (var file in updated)
            {
                WriteHelpText($"    {file}");
            }
        }
    }
}
