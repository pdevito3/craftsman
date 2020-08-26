namespace Craftsman.CraftsmanOptions
{
    using CommandLine;

    public class AddPropertyOptions
    {

        [Option('e', "entity", Required = true, HelpText = "Name of the entity to add the property. Must match the name of the entity file (e.g. `Vet.cs` should be `Vet`)")]
        public string Entity { get; set; }

        [Option('n', "name", Required = true, HelpText = "Name of the property to add")]
        public string Name { get; set; }

        [Option('t', "type", Required = true, HelpText = "Name of the property to add")]
        public string Type { get; set; }

        [Option('f', "filter", Required = false, HelpText = "Determines if the property is filterable")]
        public bool CanFilter { get; set; }

        [Option('s', "sort", Required = false, HelpText = "Determines if the property is sortable")]
        public bool CanSort { get; set; }

        [Option('k', "foreignkey", Required = false, HelpText = "When adding an object linked by a foreign key, use this field to enter the name of the property that acts as the foreign key")]
        public string ForeignKeyPropName { get; set; }
    }
}
