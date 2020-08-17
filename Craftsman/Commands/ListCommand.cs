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

            WriteHelpHeader(@$"Commands:");

            WriteHelpHeader(@$"    assistance");
            WriteHelpText(@$"      list                List commands");
            WriteHelpText(@$"      help                Displays help for a command");

            WriteHelpHeader(@$"{Environment.NewLine}    add");
            WriteHelpText(@$"      add:entity          Add a new entity to your API Project");
            WriteHelpText(@$"      add:property        Adds a new property to an entity in your API Project");

            WriteHelpHeader(@$"{Environment.NewLine}    new");
            WriteHelpText(@$"      new:api             Create a new API Project");

            WriteHelpHeader(@$"{Environment.NewLine}Options:");
            WriteHelpText(@$"   -h, --help          Display this help message");

            WriteHelpHeader(@$"{Environment.NewLine}Example Help Option Usage:");
            WriteHelpText(@$"   craftsman new:api -h");
            WriteHelpText(@$"   craftsman add:entity -h");
            WriteHelpText(@$"   craftsman add:property -h");

        }
    }
}
