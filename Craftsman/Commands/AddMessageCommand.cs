namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Domain;
using Exceptions;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Validators;

public class AddMessageCommand(
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IAnsiConsole console,
    IFileParsingHelper fileParsingHelper,
    IMediator mediator)
    : Command<AddMessageCommand.Settings>
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly IAnsiConsole _console = console;

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")]
        public string Filepath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialSolutionDir = utilities.GetRootDir();

        utilities.IsSolutionDirectoryGuard(potentialSolutionDir);
        scaffoldingDirectoryStore.SetSolutionDirectory(potentialSolutionDir);

        fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var template = fileParsingHelper.GetTemplateFromFile<MessageTemplate>(settings.Filepath);
        consoleWriter.WriteHelpText($"Your template file was parsed successfully.");

        AddMessages(scaffoldingDirectoryStore.SolutionDirectory, template.Messages);

        consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    public void AddMessages(string solutionDirectory, List<Message> messages)
    {
        var validator = new MessageValidator();
        foreach (var message in messages)
        {
            var results = validator.Validate(message);
            if (!results.IsValid)
                throw new DataValidationErrorException(results.Errors);
        }

        foreach (var message in messages)
        {
            mediator.Send(new MessageBuilder.MessageBuilderCommand(solutionDirectory, message)).GetAwaiter().GetResult();
        }
    }
}