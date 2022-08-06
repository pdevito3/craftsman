namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class CommonValueObjectBuilder
{
    public class CommonValueObjectBuilderCommand : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<CommonValueObjectBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(CommonValueObjectBuilderCommand request, CancellationToken cancellationToken)
        {
            var percentClassPath = ClassPathHelper.WebApiValueObjectsClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{ValueObjectEnum.Percent.Name}.cs",
                ValueObjectEnum.Percent.Plural(),
                _scaffoldingDirectoryStore.ProjectBaseName);
            var percentFileText = GetPercentFileText(percentClassPath.ClassNamespace);
            _utilities.CreateFile(percentClassPath, percentFileText);
            
            var addressClassPath = ClassPathHelper.WebApiValueObjectsClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{ValueObjectEnum.Address.Name}.cs",
                ValueObjectEnum.Address.Plural(),
                _scaffoldingDirectoryStore.ProjectBaseName);
            var addressFileText = GetAddressFileText(addressClassPath.ClassNamespace);
            _utilities.CreateFile(addressClassPath, addressFileText);
            
            var monetaryAmountClassPath = ClassPathHelper.WebApiValueObjectsClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{ValueObjectEnum.MonetaryAmount.Name}.cs",
                ValueObjectEnum.MonetaryAmount.Plural(),
                _scaffoldingDirectoryStore.ProjectBaseName);
            var monetaryAmountFileText = GetMonetaryAmountFileText(monetaryAmountClassPath.ClassNamespace);
            _utilities.CreateFile(monetaryAmountClassPath, monetaryAmountFileText);

            return Task.FromResult(true);
        }
        
        private string GetPercentFileText(string classNamespace)
        {
            var voClassPath = ClassPathHelper.SharedKernelDomainClassPath(_scaffoldingDirectoryStore.SolutionDirectory, "");
            
            return @$"namespace {classNamespace};

using {voClassPath.ClassNamespace};
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
        }
        
        private string GetAddressFileText(string classNamespace)
        {
            var voClassPath = ClassPathHelper.SharedKernelDomainClassPath(_scaffoldingDirectoryStore.SolutionDirectory, "");
            
            return @$"namespace {classNamespace};

using {voClassPath.ClassNamespace};
using FluentValidation;

public class {ValueObjectEnum.Address.Name} : ValueObject
{{
    /// <summary>
    /// Address line 1 (e.g., street, PO Box, or company name).
    /// </summary>
    public string Line1 {{ get; }}
    
    /// <summary>
    /// Address line 2 (e.g., apartment, suite, unit, or building).
    /// </summary>
    public string Line2 {{ get; }}
    
    /// <summary>
    /// City, district, suburb, town, or village.
    /// </summary>
    public string City {{ get; }}
    
    /// <summary>
    /// State, county, province, or region.
    /// </summary>
    public string State {{ get; }}
    
    /// <summary>
    /// ZIP or postal code.
    /// </summary>
    public PostalCode PostalCode {{ get; }}
    
    /// <summary>
    /// Two-letter country code <a href=""https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2"">(ISO 3166-1 alpha-2)</a>.
    /// </summary>
    public string Country {{ get; }}
    
    public {ValueObjectEnum.Address.Name}(string line1, string line2, string city, string state, string postalCode, string country)
        : this(line1, line2, city, state, PostalCode.Of(postalCode), country)
    {{
    }}

    public {ValueObjectEnum.Address.Name}(string line1, string line2, string city, string state, PostalCode postalCode, string country)
    {{
        // TODO country validation

        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }}
}}

public class PostalCode : ValueObject
{{
    public string Value {{ get; }}
    public PostalCode(string value)
    {{
        Value = value;
    }}

    public static PostalCode Of(string postalCode) => new PostalCode(postalCode);
    public static implicit operator string(PostalCode postalCode) => postalCode.Value;
}}";
        }
        
        private string GetMonetaryAmountFileText(string classNamespace)
        {
            var percentClassPath = ClassPathHelper.WebApiValueObjectsClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{ValueObjectEnum.Percent.Name}.cs",
                ValueObjectEnum.Percent.Plural(),
                _scaffoldingDirectoryStore.ProjectBaseName);
            var voClassPath = ClassPathHelper.SharedKernelDomainClassPath(_scaffoldingDirectoryStore.SolutionDirectory, "");
            
            return @$"namespace {classNamespace};

using {percentClassPath.ClassNamespace};
using {voClassPath.ClassNamespace};
using FluentValidation;

// source: https://github.com/asc-lab/better-code-with-ddd/blob/ef_core/LoanApplication.TacticalDdd/LoanApplication.TacticalDdd/DomainModel/MonetaryAmount.cs
public class {ValueObjectEnum.MonetaryAmount.Name} : ValueObject
{{
    public decimal Amount {{ get; }}
        
    public static readonly MonetaryAmount Zero = new MonetaryAmount(0M);

    public MonetaryAmount(decimal amount) => Amount = decimal.Round(amount,2,MidpointRounding.ToEven);

    public MonetaryAmount Add(MonetaryAmount other) => new MonetaryAmount(Amount + other.Amount);

    public MonetaryAmount Subtract(MonetaryAmount other) => new MonetaryAmount(Amount - other.Amount);
        
    public MonetaryAmount MultiplyByPercent(Percent percent) => new MonetaryAmount((Amount * percent.Value)/100M);

    public static MonetaryAmount operator +(MonetaryAmount one, MonetaryAmount two) => one.Add(two);
        
    public static MonetaryAmount operator -(MonetaryAmount one, MonetaryAmount two) => one.Subtract(two);
        
    public static MonetaryAmount operator *(MonetaryAmount one, Percent percent) => one.MultiplyByPercent(percent);
        
    public static bool operator >(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)>0;
        
    public static bool operator <(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)<0;
        
    public static bool operator >=(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)>=0;
        
    public static bool operator <=(MonetaryAmount one, MonetaryAmount two) => one.CompareTo(two)<=0;

    public int CompareTo(MonetaryAmount other)
    {{
        return Amount.CompareTo(other.Amount);
    }}
}}";
        }
    }
}
