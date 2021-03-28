namespace Craftsman.CraftsmanOptions
{
    using CommandLine;

    public class AddEntityOptions : IVerbosable
    {

        [Option('v', Required = false, HelpText = "Show verbose output.")]

        public bool Verbosity { get; set; }
    }
}
