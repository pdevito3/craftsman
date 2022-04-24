namespace Craftsman.Domain;

using Ardalis.SmartEnum;

public abstract class NamingConventionEnum : SmartEnum<NamingConventionEnum>
{
    public static readonly NamingConventionEnum Class = new ClassConventionEnum();
    public static readonly NamingConventionEnum SnakeCase = new SnakeCaseConventionEnum();
    public static readonly NamingConventionEnum LowerCase = new LowerCaseConventionEnum();
    public static readonly NamingConventionEnum CamelCase = new CamelCaseConventionEnum();
    public static readonly NamingConventionEnum UpperCase = new UpperCaseConventionEnum();

    protected NamingConventionEnum(string name, int value) : base(name, value)
    {
    }
    /// <summary>
    /// The extenstion method for the naming convention.
    /// </summary>
    /// <returns>Only returns the name, so would return `UseSnakeCaseNamingConvention` instead of `.UseSnakeCaseNamingConvention()`</returns>
    public abstract string ExtensionMethod();

    private class ClassConventionEnum : NamingConventionEnum
    {
        public ClassConventionEnum() : base(nameof(Class), 1) { }

        public override string ExtensionMethod() => null;
    }

    private class SnakeCaseConventionEnum : NamingConventionEnum
    {
        public SnakeCaseConventionEnum() : base(nameof(SnakeCase), 2) { }

        public override string ExtensionMethod() => "UseSnakeCaseNamingConvention";
    }

    private class LowerCaseConventionEnum : NamingConventionEnum
    {
        public LowerCaseConventionEnum() : base(nameof(LowerCase), 3) { }

        public override string ExtensionMethod() => "UseLowerCaseNamingConvention";
    }

    private class CamelCaseConventionEnum : NamingConventionEnum
    {
        public CamelCaseConventionEnum() : base(nameof(CamelCase), 4) { }

        public override string ExtensionMethod() => "UseCamelCaseNamingConvention";
    }


    private class UpperCaseConventionEnum : NamingConventionEnum
    {
        public UpperCaseConventionEnum() : base(nameof(UpperCase), 5) { }

        public override string ExtensionMethod() => "UseUpperCaseNamingConvention";
    }
}
