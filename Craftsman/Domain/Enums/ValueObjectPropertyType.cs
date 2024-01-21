namespace Craftsman.Domain.Enums;

using System;
using Ardalis.SmartEnum;
using Helpers;
using Humanizer;
using Services;

public abstract class ValueObjectPropertyType : SmartEnum<ValueObjectPropertyType>
{
    public static ValueObjectPropertyType None(string className) => new NoneType(className);
    public static ValueObjectPropertyType Simple(string className) => new SimpleType(className);
    public static ValueObjectPropertyType Smart(string className) => new SmartType(className);
    public static ValueObjectPropertyType Email(string className) => new EmailType(className);
    public static ValueObjectPropertyType Percent(string className) => new PercentType(className);
    public static ValueObjectPropertyType MonetaryAmount(string className) => new MonetaryAmountType(className);
    
    public bool IsNone => Value == 0;
    public bool IsSimple => Value == 1;
    public bool IsSmart => Value == 2;
    public bool IsEmail => Value == 3;
    public bool IsPercent => Value == 4;
    public bool IsMonetaryAmount => Value == 5;

    public string ClassName { get; protected set; }
    protected ValueObjectPropertyType(string name, int value, string className) : base(name, value)
    {
        ClassName = className;
    }
    public abstract string GetFileText(string classNamespace, 
        string valueObjectClassName, string propertyType,
        List<string> smartNames, string srcDirectory, string projectBaseName);
    public abstract string GetEntityCreationSetter(string createEntityVar, string propertyName,
        string creationModelVarName);
    public abstract string GetEntityUpdateSetter(string propertyName, string updateModelVarName);
    public abstract string GetDbConfig(string propertyName);
    public abstract string GetMapperAttribute(string entityName, string entityPropertyName);
    public abstract string GetIntegratedCreationDomainAssertion(string lowercaseEntityName, string propertyName,
        string fakeEntityVariableName, string propertyType);
    public abstract string GetIntegratedCreationReturnedAssertion(string lowercaseEntityName, string propertyName,
        string fakeEntityVariableName, string propertyType);
    public abstract string GetIntegratedReadRecordAssertion(string lowercaseEntityName, string propertyName, 
        string fakeEntityVariableName, string propertyType);
    public abstract string GetIntegratedUpdatedRecordAssertion(string lowercaseEntityName, string propertyName,
        string fakeEntityVariableName, string propertyType);
    
    private class NoneType : ValueObjectPropertyType
    {
        public NoneType(string className) : base("None", 0, className) { }

        public override string GetFileText(string classNamespace, string valueObjectClassName, string propertyType,
            List<string> smartNames, string srcDirectory, string projectBaseName) => null;
        
        public override string GetEntityCreationSetter(string createEntityVar, string propertyName,
            string creationModelVarName) => null;
        
        public override string GetEntityUpdateSetter(string propertyName, string updateModelVarName) => null;
        public override string GetDbConfig(string propertyName) => null;
        public override string GetMapperAttribute(string entityName, string entityPropertyName) => null;
        
        public override string GetIntegratedCreationReturnedAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => null;
        
        public override string GetIntegratedCreationDomainAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => null;
        
        public override string GetIntegratedReadRecordAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => null;

        public override string GetIntegratedUpdatedRecordAssertion(string lowercaseEntityName, string propertyName,
            string fakeEntityVariableName, string propertyType)
            => null;
    }
    
    private class SimpleType : ValueObjectPropertyType
    {
        public SimpleType(string className) : base("Simple", 1, className) { }

        public override string GetFileText(string classNamespace, string valueObjectClassName, string propertyType,
            List<string> smartNames, string srcDirectory, string projectBaseName)
        {
            var toString = propertyType is "string" or "string?" 
                ? "" 
                : $@".ToString()";
            return @$"namespace {classNamespace};

using FluentValidation;

public sealed class {valueObjectClassName} : ValueObject
{{
    public {propertyType} Value {{ get; set; }}
    
    public {valueObjectClassName}({propertyType} value)
    {{
        Value = value;
    }}
    
    public static {valueObjectClassName} Of({propertyType} value) => new {valueObjectClassName}(value);
    public static implicit operator string({valueObjectClassName} value) => value.Value{toString};

    private {valueObjectClassName}() {{ }} // EF Core
    
    private sealed class {valueObjectClassName}Validator : AbstractValidator<{propertyType}> 
    {{
        public {valueObjectClassName}Validator()
        {{
        }}
    }}
}}";
        }
        
        public override string GetEntityCreationSetter(string createEntityVar, string propertyName,
            string creationModelVarName)
            => $"        {createEntityVar}.{propertyName} = {ClassName}.Of({creationModelVarName}.{propertyName});";
        
        public override string GetEntityUpdateSetter(string propertyName, string updateModelVarName)
            => $"        {propertyName} = {ClassName}.Of({updateModelVarName}.{propertyName});";
        
        public override string GetDbConfig(string propertyName)
        //     => $@"
        //             
        // builder.OwnsOne(x => x.{propertyName}, opts =>
        //     {{
        //         opts.Property(x => x.Value);
        //     }}).Navigation(x => x.{propertyName})
        //     .IsRequired();";
            => $@"

        builder.Property(x => x.{propertyName})
            .HasConversion(x => x.Value, x => new {ClassName}(x));";
        

        public override string GetMapperAttribute(string entityName, string entityPropertyName)
            => GetMapperAttributeValue(entityName, entityPropertyName, "Value");
        public override string GetIntegratedCreationReturnedAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => GetCreationReturnedAssertion(lowercaseEntityName, propertyName, fakeEntityVariableName, propertyType);
        public override string GetIntegratedCreationDomainAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedReadRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetReadRecordAssertion(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedUpdatedRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
    }

    private class SmartType : ValueObjectPropertyType
    {
        public SmartType(string className) : base("Smart", 2, className) { }

        public override string GetFileText(string classNamespace, string valueObjectClassName, string propertyType,
            List<string> smartNames, string srcDirectory, string projectBaseName)
        {
            var valueObjectClassNameLower = valueObjectClassName.LowercaseFirstLetter();
            var staticEnums = GetStaticEnums(valueObjectClassName, smartNames);
            var smartEnum = GetSmartEnum(valueObjectClassName, smartNames);
            var enumClassName = $"{valueObjectClassName}Enum";
            var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName); 
            
            return @$"namespace {classNamespace};

using Ardalis.SmartEnum;
using {exceptionClassPath.ClassNamespace};

public sealed class {valueObjectClassName} : ValueObject
{{
    private {enumClassName} _{valueObjectClassNameLower};
    public string Value
    {{
        get => _{valueObjectClassNameLower}.Name;
        private set
        {{
            if (!{enumClassName}.TryFromName(value, true, out var parsed))
                throw new ValidationException($""Invalid {valueObjectClassName.Humanize()}. PLease use one of the following: {{string.Join("", "", ListNames())}}"");

            _{valueObjectClassNameLower} = parsed;
        }}
    }}
    
    public {valueObjectClassName}(string value)
    {{
        Value = value;
    }}

    public static {valueObjectClassName} Of(string value) => new {valueObjectClassName}(value);
    public static implicit operator string({valueObjectClassName} value) => value.Value;
    public static List<string> ListNames() => {enumClassName}.List.Select(x => x.Name).ToList();

{staticEnums}

    private {valueObjectClassName}() {{ }} // EF Core

{smartEnum}
}}";
        }

        private static List<string> GetNormalizedSmartNames(List<string> smartNames)
            => smartNames.Select(NormalizeSmartName).ToList();
        
        private static string NormalizeSmartName(string smartName)
            => smartName.Trim()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(":", "")
                .Replace(";", "");
        
        private static string GetStaticEnums(string propertyName, List<string> normalizedSmartNames)
        {
            var staticEnums = normalizedSmartNames.Select(s => $@"   public static {propertyName} {NormalizeSmartName(s)}() => new {propertyName}({propertyName}Enum.{NormalizeSmartName(s)}.Name);");
            
            return string.Join($"{Environment.NewLine}", staticEnums);
        }
        
        private static string GetSmartEnum(string valueObjectClassName, List<string> smartNames)
        {
            var staticProps =
                smartNames.Select(s => $@"      public static readonly {valueObjectClassName}Enum {NormalizeSmartName(s)} = new {NormalizeSmartName(s)}Type();");
            var eachEnum = smartNames.Select(s => $@"       private class {NormalizeSmartName(s)}Type() : {valueObjectClassName}Enum(""{s}"", {smartNames.IndexOf(s)});");

            return $@"    private abstract class {valueObjectClassName}Enum(string name, int value)
        : SmartEnum<{valueObjectClassName}Enum>(name, value)
    {{
{string.Join($"{Environment.NewLine}", staticProps)}

{string.Join($"{Environment.NewLine}{Environment.NewLine}", eachEnum)}
    }}";
        }
        
        public override string GetEntityCreationSetter(string createEntityVar, string propertyName,
            string creationModelVarName)
            => $"        {createEntityVar}.{propertyName} = {ClassName}.Of({creationModelVarName}.{propertyName});";
        
        public override string GetEntityUpdateSetter(string propertyName, string updateModelVarName)
            => $"        {propertyName} = {ClassName}.Of({updateModelVarName}.{propertyName});";
        
        public override string GetDbConfig(string propertyName)
        //     => $@"
        //             
        // builder.OwnsOne(x => x.{propertyName}, opts =>
        //     {{
        //         opts.Property(x => x.Value);
        //     }}).Navigation(x => x.{propertyName})
        //     .IsRequired();";
            => $@"

        builder.Property(x => x.{propertyName})
            .HasConversion(x => x.Value, x => new {ClassName}(x));";
        

        public override string GetMapperAttribute(string entityName, string entityPropertyName)
            => GetMapperAttributeValue(entityName, entityPropertyName, "Value");
        public override string GetIntegratedCreationReturnedAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => GetCreationReturnedAssertion(lowercaseEntityName, propertyName, fakeEntityVariableName, propertyType);
        public override string GetIntegratedCreationDomainAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedReadRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetReadRecordAssertion(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedUpdatedRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
    }

    public class EmailType : ValueObjectPropertyType
    {
        public EmailType(string className) : base("Email", 3, className) { }

        public override string GetFileText(string classNamespace, string valueObjectClassName, string propertyType,
            List<string> smartNames, string srcDirectory, string projectBaseName)
            => @$"namespace {classNamespace};

using FluentValidation;

public sealed class Email : ValueObject
{{
    public string Value {{ get; private set; }}
    
    public Email(string value)
    {{
        if (string.IsNullOrWhiteSpace(value))
        {{
            Value = null;
            return;
        }}
        new EmailValidator().ValidateAndThrow(value);
        Value = value;
    }}
    
    public static Email Of(string value) => new Email(value);
    public static implicit operator string(Email value) => value.Value;

    private Email() {{ }} // EF Core
    
    private sealed class EmailValidator : AbstractValidator<string> 
    {{
        public EmailValidator()
        {{
            RuleFor(email => email).EmailAddress();
        }}
    }}
}}";
        
        public override string GetEntityCreationSetter(string createEntityVar, string propertyName,
            string creationModelVarName)
            => $@"        {createEntityVar}.{propertyName} = {ClassName}.Of({creationModelVarName}.{propertyName});";
        
        public override string GetEntityUpdateSetter(string propertyName, string updateModelVarName)
            => $@"        {propertyName} = {ClassName}.Of({updateModelVarName}.{propertyName});";

        public override string GetDbConfig(string propertyName)
            //     => $@"
            //             
            // builder.OwnsOne(x => x.{propertyName}, opts =>
            //     {{
            //         opts.Property(x => x.Value);
            //     }}).Navigation(x => x.{propertyName})
            //     .IsRequired();";
            => $@"

        builder.Property(x => x.{propertyName})
            .HasConversion(x => x.Value, x => new {ClassName}(x));";
        

        public override string GetMapperAttribute(string entityName, string entityPropertyName)
            => GetMapperAttributeValue(entityName, entityPropertyName, "Value");
        public override string GetIntegratedCreationReturnedAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => GetCreationReturnedAssertion(lowercaseEntityName, propertyName, fakeEntityVariableName, propertyType);
        public override string GetIntegratedCreationDomainAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedReadRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetReadRecordAssertion(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedUpdatedRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
    }

    public class PercentType : ValueObjectPropertyType
    {
        public PercentType(string className) : base("Percent", 4, className) { }

        public override string GetFileText(string classNamespace, string valueObjectClassName, string propertyType,
            List<string> smartNames, string srcDirectory, string projectBaseName)
            => @$"namespace {classNamespace};

using FluentValidation;

// source: https://github.com/asc-lab/better-code-with-ddd/blob/ef_core/LoanApplication.TacticalDdd/LoanApplication.TacticalDdd/DomainModel/Percent.cs
public class {ValueObjectEnum.Percent.Name} : ValueObject
{{
    public decimal Value {{ get; }}
        
    public static readonly Percent Zero = new Percent(0M);

    public Percent(decimal value)
    {{
        if (value < 0)
            throw new ArgumentException(""Percent value cannot be negative"");

        Value = value;
    }}
        
    public static bool operator >(Percent one, Percent two) => one.CompareTo(two)>0;
        
    public static bool operator <(Percent one, Percent two) => one.CompareTo(two)<0;
        
    public static bool operator >=(Percent one, Percent two) => one.CompareTo(two)>=0;
        
    public static bool operator <=(Percent one, Percent two) => one.CompareTo(two)<=0;

    public static Percent Of(decimal value) => new Percent(value);

    private int CompareTo(Percent other)
    {{
        return Value.CompareTo(other.Value);
    }}

    protected Percent() {{ }} // EF Core
}}

public static class PercentExtensions
{{
    public static Percent Percent(this int value) => new Percent(value);
        
    public static Percent Percent(this decimal value) => new Percent(value);
}}";
        
        public override string GetEntityCreationSetter(string createEntityVar, string propertyName,
            string creationModelVarName)
            => $@"        {createEntityVar}.{propertyName} = {ClassName}.Of({creationModelVarName}.{propertyName});";
        
        public override string GetEntityUpdateSetter(string propertyName, string updateModelVarName)
            => $@"        {propertyName} = {ClassName}.Of({updateModelVarName}.{propertyName});";
        
        public override string GetDbConfig(string propertyName)
        //     => $@"
        //             
        // builder.OwnsOne(x => x.{propertyName}, opts =>
        //     {{
        //         opts.Property(x => x.Value);
        //     }}).Navigation(x => x.{propertyName})
        //     .IsRequired();";
            => $@"

        builder.Property(x => x.{propertyName})
            .HasConversion(x => x.Value, x => new {ClassName}(x));";
        
        public override string GetMapperAttribute(string entityName, string entityPropertyName)
            => GetMapperAttributeValue(entityName, entityPropertyName, "Value");
        public override string GetIntegratedCreationReturnedAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => GetCreationReturnedAssertion(lowercaseEntityName, propertyName, fakeEntityVariableName, propertyType);
        public override string GetIntegratedCreationDomainAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedReadRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetReadRecordAssertion(actualVarName, propertyName, expectedVarName, "Value", propertyType);
        public override string GetIntegratedUpdatedRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Value", propertyType);
    }

    public class MonetaryAmountType : ValueObjectPropertyType
    {
        public MonetaryAmountType(string className) : base("MonetaryAmount", 5, className) { }

        public override string GetFileText(string classNamespace, string valueObjectClassName, string propertyType,
            List<string> smartNames, string srcDirectory, string projectBaseName)
            => @$"namespace {classNamespace};

using Percentages;
using FluentValidation;

// source: https://github.com/asc-lab/better-code-with-ddd/blob/ef_core/LoanApplication.TacticalDdd/LoanApplication.TacticalDdd/DomainModel/MonetaryAmount.cs
public class MonetaryAmount : ValueObject
{{
    public decimal Amount {{ get; }}
        
    public static readonly MonetaryAmount Zero = new MonetaryAmount(0M);

    public MonetaryAmount(decimal amount) => Amount = decimal.Round(amount,2,MidpointRounding.ToEven);

    public MonetaryAmount Add(MonetaryAmount other) => new MonetaryAmount(Amount + other.Amount);

    public MonetaryAmount Add(decimal amount) => Add(new MonetaryAmount(amount));

    public MonetaryAmount Subtract(MonetaryAmount other) => new MonetaryAmount(Amount - other.Amount);

    public MonetaryAmount Subtract(decimal amount) => Subtract(new MonetaryAmount(amount));
        
    public MonetaryAmount MultiplyByPercent(Percent percent) => new MonetaryAmount((Amount * percent.Value)/100M);

    public MonetaryAmount MultiplyByPercent(decimal percent) => MultiplyByPercent(new Percent(percent));

    public static MonetaryAmount operator +(MonetaryAmount one, MonetaryAmount two) => one.Add(two);
        
    public static MonetaryAmount operator -(MonetaryAmount one, MonetaryAmount two) => one.Subtract(two);
        
    public static MonetaryAmount operator *(MonetaryAmount one, Percent percent) => one.MultiplyByPercent(percent);
        
    public static bool operator >(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)>0;
        
    public static bool operator <(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)<0;
        
    public static bool operator >=(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)>=0;
        
    public static bool operator <=(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)<=0;
    
    public static MonetaryAmount Of(decimal value) => new MonetaryAmount(value);

    public int CompareTo(MonetaryAmount other)
    {{
        return Amount.CompareTo(other.Amount);
    }}
}}";
        
        public override string GetEntityCreationSetter(string createEntityVar, string propertyName,
            string creationModelVarName)
            => $@"        {createEntityVar}.{propertyName} = {ClassName}.Of({creationModelVarName}.{propertyName});";
        
        public override string GetEntityUpdateSetter(string propertyName, string updateModelVarName)
            => $@"        {propertyName} = {ClassName}.Of({updateModelVarName}.{propertyName});";
        
        public override string GetDbConfig(string propertyName)
        //     => $@"
        //             
        // builder.OwnsOne(x => x.{propertyName}, opts =>
        //     {{
        //         opts.Property(x => x.Amount);
        //     }}).Navigation(x => x.{propertyName})
        //     .IsRequired();";
            => $@"

        builder.Property(x => x.{propertyName})
            .HasConversion(x => x.Amount, x => new {ClassName}(x));";
        
        public override string GetMapperAttribute(string entityName, string entityPropertyName)
            => GetMapperAttributeValue(entityName, entityPropertyName, "Amount");
        public override string GetIntegratedCreationReturnedAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string propertyType)
            => GetCreationReturnedAssertion(lowercaseEntityName, propertyName, fakeEntityVariableName, propertyType);
        public override string GetIntegratedCreationDomainAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Amount", propertyType);
        public override string GetIntegratedReadRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetReadRecordAssertion(actualVarName, propertyName, expectedVarName, "Amount", propertyType);
        public override string GetIntegratedUpdatedRecordAssertion(string actualVarName, string propertyName, string expectedVarName, string propertyType)
            => GetDomainAssertionForCreateOrUpdate(actualVarName, propertyName, expectedVarName, "Amount", propertyType);
    }

    private static string GetCreationReturnedAssertion(string lowercaseEntityName, string propertyName,
        string fakeEntityVariableName, string propertyType)
    {
        return propertyType switch
        {
            "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo({fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "DateTime?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo((DateTime){fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "DateTimeOffset?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "TimeOnly?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo((TimeOnly){fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "decimal" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately({fakeEntityVariableName}.{propertyName}, 0.005M);",
            "decimal?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately((decimal){fakeEntityVariableName}.{propertyName}, 0.005M);",
            "float" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately({fakeEntityVariableName}.{propertyName}, 0.005F);",
            "float?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately((float){fakeEntityVariableName}.{propertyName}, 0.005F);",
            _ =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().Be({fakeEntityVariableName}.{propertyName});"
        };
    }
    
    private static string GetDomainAssertionForCreateOrUpdate(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string voPropName, string propertyType)
    {
        return propertyType switch
        {
            "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeCloseTo({fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "DateTime?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeCloseTo((DateTime){fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "DateTimeOffset?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "TimeOnly?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeCloseTo((TimeOnly){fakeEntityVariableName}.{propertyName}, 1.Seconds());",
            "decimal" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeApproximately({fakeEntityVariableName}.{propertyName}, 0.005M);",
            "decimal?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeApproximately((decimal){fakeEntityVariableName}.{propertyName}, 0.005M);",
            "float" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeApproximately({fakeEntityVariableName}.{propertyName}, 0.005F);",
            "float?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().BeApproximately((float){fakeEntityVariableName}.{propertyName}, 0.005F);",
            _ =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.{voPropName}.Should().Be({fakeEntityVariableName}.{propertyName});"
        };
    }
    
    private static string GetReadRecordAssertion(string lowercaseEntityName, string propertyName, string fakeEntityVariableName, string voPropName, string propertyType)
    {
        return propertyType switch
        {
            "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo({fakeEntityVariableName}.{propertyName}.{voPropName}, 1.Seconds());",
            "DateTime?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo((DateTime){fakeEntityVariableName}.{propertyName}.{voPropName}, 1.Seconds());",
            "DateTimeOffset?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo((DateTimeOffset){fakeEntityVariableName}.{propertyName}.{voPropName}, 1.Seconds());",
            "TimeOnly?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeCloseTo((TimeOnly){fakeEntityVariableName}.{propertyName}.{voPropName}, 1.Seconds());",
            "decimal" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately({fakeEntityVariableName}.{propertyName}.{voPropName}, 0.005M);",
            "decimal?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately((decimal){fakeEntityVariableName}.{propertyName}.{voPropName}, 0.005M);",
            "float" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately({fakeEntityVariableName}.{propertyName}.{voPropName}, 0.005F);",
            "float?" =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().BeApproximately((float){fakeEntityVariableName}.{propertyName}.{voPropName}, 0.005F);",
            _ =>
                $@"{Environment.NewLine}        {lowercaseEntityName}.{propertyName}.Should().Be({fakeEntityVariableName}.{propertyName}.{voPropName});"
        };
    }

    private static string GetMapperAttributeValue(string entityName, string entityPropertyName, string voPropName)
        =>
            $@"    [MapProperty(new[] {{ nameof({entityName}.{entityPropertyName}), nameof({entityName}.{entityPropertyName}.{voPropName}) }}, new[] {{ nameof({entityName}Dto.{entityPropertyName}) }})]
";
}
