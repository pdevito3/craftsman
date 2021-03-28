namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using static Helpers.ConsoleWriter;

    public static class AddEntityPropertyCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   While in your project directory, this command will add a new property to an entity.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:property [options] [arguments]");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   -e, -entity        Required. Text. Name of the entity to add the property. 
                      Must match the name of the entity file (e.g. `Vet.cs` should 
                      be `Vet`)");
            WriteHelpText(@$"   -n, -name          Required. Text. Name of the property to add");
            WriteHelpText(@$"   -t, -type          Required. Text. Data type of the property to add");
            WriteHelpText(@$"   -f, -filter        Optional. Boolean. Determines if the property is filterable");
            WriteHelpText(@$"   -s, -sort          Optional. Boolean. Determines if the property is sortable");
            WriteHelpText(@$"   -k, -foreignkey    Optional. Text. When adding an object linked by a foreign 
                      key, use this field to enter the name of the property that 
                      acts as the foreign key");

            // add new line back in if adding something with only one line of text after foreign key above
            //WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No Arguments are needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"       craftsman add:property --entity Vet --name VetName --type string --filter false --sort true");
            WriteHelpText(@$"       craftsman add:property -e Vet -n VetName -t string -f false -s true");
            WriteHelpText(@$"       craftsman add:property -e Vet -n VetName -t string");
            WriteHelpText(@$"       craftsman add:property -e Sale -n Product -t Product -k ProductId");
            WriteHelpText(@$"       craftsman add:property -e Vet -n AppointmentDate -t DateTime? -f false -s true");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string solutionDirectory, string entityName, EntityProperty prop)
        {
            try
            {
                var propList = new List<EntityProperty>() { prop };
                
                EntityModifier.AddEntityProperties(solutionDirectory, entityName, propList);
                DtoModifier.AddPropertiesToDtos(solutionDirectory, entityName, propList, "EntityBrokenHere");

                WriteHelpHeader($"{Environment.NewLine}The '{prop.Name}' property was successfully added to the '{entityName}' entity and it's associated DTOs. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is FileAlreadyExistsException
                    || e is DirectoryAlreadyExistsException
                    || e is InvalidSolutionNameException
                    || e is FileNotFoundException
                    || e is InvalidDbProviderException
                    || e is InvalidFileTypeException
                    || e is SolutionNotFoundException)
                {
                    WriteError($"{e.Message}");
                }
                else
                    WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
            }
        }
    }
}
