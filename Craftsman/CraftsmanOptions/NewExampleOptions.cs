namespace Craftsman.CraftsmanOptions
{
    using CommandLine;

    public class NewExampleOptions
    {
        [Option('n', "name", Required = true, HelpText = "Name of the example project")]
        public string Name { get; set; }

        [Option('t', "type", Required = true, HelpText = "Type of the example you want to create")]
        public string Type { get; set; }
        
    }
}
