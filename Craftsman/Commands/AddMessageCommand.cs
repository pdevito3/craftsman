namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Validators;

    public static class AddMessageCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command will add a messageto your messages project using a formatted yaml or json file.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:message [options] <filepath>");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that lists the message information that you want to add to your project.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman add:message C:\fullpath\mymessageinfo.yaml");
            WriteHelpText(@$"   craftsman add:message C:\fullpath\mymessageinfo.yml");
            WriteHelpText(@$"   craftsman add:message C:\fullpath\mymessageinfo.json");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string filePath, string solutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<MessageTemplate>(filePath);

                // get solution dir
                Utilities.IsSolutionDirectoryGuard(solutionDirectory);
                AddMessages(solutionDirectory, fileSystem, template.Messages);

                WriteHelpHeader($"{Environment.NewLine}Your messages have been successfully added. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is SolutionNotFoundException
                    || e is DataValidationErrorException)
                {
                    WriteError($"{e.Message}");
                }
                else
                {
                    AnsiConsole.WriteException(e, new ExceptionSettings
                    {
                        Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                        Style = new ExceptionStyle
                        {
                            Exception = new Style().Foreground(Color.Grey),
                            Message = new Style().Foreground(Color.White),
                            NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                            Parenthesis = new Style().Foreground(Color.Cornsilk1),
                            Method = new Style().Foreground(Color.Red),
                            ParameterName = new Style().Foreground(Color.Cornsilk1),
                            ParameterType = new Style().Foreground(Color.Red),
                            Path = new Style().Foreground(Color.Red),
                            LineNumber = new Style().Foreground(Color.Cornsilk1),
                        }
                    });
                }
            }
        }

        public static void AddMessages(string solutionDirectory, IFileSystem fileSystem, List<Message> messages)
        {
            var validator = new MessageValidator();
            foreach (var message in messages)
            {
                var results = validator.Validate(message);
                if (!results.IsValid)
                    throw new DataValidationErrorException(results.Errors);
            }

            messages.ForEach(message => MessageBuilder.CreateMessage(solutionDirectory, message, fileSystem));
        }
    }
}