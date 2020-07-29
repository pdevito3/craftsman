namespace Craftsman
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ConsoleWriter
    {
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
    }
}
