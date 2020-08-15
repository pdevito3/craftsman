namespace Craftsman.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public static class ListCommand
    {
        public static void Run()
        {
            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   command [options] [arguments]{Environment.NewLine}");

            WriteHelpHeader(@$"Available commands:");
            WriteHelpText(@$"   list                List commands");
            WriteHelpText(@$"   help                Displays help for a command");

            WriteHelpHeader(@$"{Environment.NewLine}add");
            WriteHelpText(@$"   add:entity          Add a new entity to your API Project");

            WriteHelpHeader(@$"{Environment.NewLine}new");
            WriteHelpText(@$"   new:api             Create a new API Project");

            //WriteHelpHeader(@$"{Environment.NewLine}make");
            //WriteHelpText(@$"   make:entity        Add a new entity and all associated files to an php existing API Project");

            //WriteHelpHeader(@$"{Environment.NewLine}make");
            //WriteHelpText(@$"   make:class        Create a new class in the current directory with a given set of properties");

            WriteHelpHeader(@$"{Environment.NewLine}Options:");
            WriteHelpText(@$"   -h, --help          Display this help message");
        }
    }
}
