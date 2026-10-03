namespace Craftsman.Commands;

using System.IO.Abstractions;
using Domain;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddBoundedContextCommand(
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IAnsiConsole console,
    IFileParsingHelper fileParsingHelper,
    IMediator mediator)
    : Command<AddBoundedContextCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")] public string Filepath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialSolutionDir = utilities.GetRootDir();

        utilities.IsSolutionDirectoryGuard(potentialSolutionDir);
        scaffoldingDirectoryStore.SetSolutionDirectory(potentialSolutionDir);

        fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var boundedContexts = fileParsingHelper.GetTemplateFromFile<BoundedContextsTemplate>(settings.Filepath);
        consoleWriter.WriteHelpText($"Your template file was parsed successfully.");

        // merge before anything is written, so a conflicting message definition fails fast
        var bcMessages = Message.MergeByName(boundedContexts.BoundedContexts
            .SelectMany(bc => bc.Messages ?? new List<Message>()));

        foreach (var template in boundedContexts.BoundedContexts)
            new ApiScaffoldingService(console, consoleWriter, utilities, scaffoldingDirectoryStore, fileSystem, mediator, fileParsingHelper)
                .ScaffoldApi(potentialSolutionDir, template);

        // bounded context messages go to the SharedKernel. Skip messages that the solution already has.
        var newBcMessages = bcMessages
            .Where(m =>
            {
                var classPath = ClassPathHelper.MessagesClassPath(potentialSolutionDir, $"{FileNames.MessageClassName(m.Name)}.cs");
                return !fileSystem.File.Exists(classPath.FullClassPath);
            })
            .ToList();

        if (newBcMessages.Count > 0)
            new AddMessageCommand(fileSystem, consoleWriter, utilities, scaffoldingDirectoryStore, console, fileParsingHelper, mediator)
                .AddMessages(potentialSolutionDir, newBcMessages);

        consoleWriter.WriteHelpHeader(
            $"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }
}