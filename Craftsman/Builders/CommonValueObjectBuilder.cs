namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class CommonValueObjectBuilder
{
    public sealed record Command(bool HasAuth) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var addressClassPath = ClassPathHelper.WebApiValueObjectsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                $"{ValueObjectEnum.Address.Name}.cs",
                ValueObjectEnum.Address.Plural(),
                scaffoldingDirectoryStore.ProjectBaseName);
            var addressFileText = GetAddressFileText(addressClassPath.ClassNamespace);
            utilities.CreateFile(addressClassPath, addressFileText);

            if (request.HasAuth)
            {
                var roleClassPath = ClassPathHelper.WebApiValueObjectsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                    $"{ValueObjectEnum.Role.Name}.cs",
                    ValueObjectEnum.Role.Plural(),
                    scaffoldingDirectoryStore.ProjectBaseName);
                var roleFileText = GetRoleFileText(roleClassPath.ClassNamespace);
                utilities.CreateFile(roleClassPath, roleFileText);
            }

            return Task.CompletedTask;
        }
        
        private string GetPercentFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

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
            
            return @$"namespace {classNamespace};

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
            var percentClassPath = ClassPathHelper.WebApiValueObjectsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                $"{ValueObjectEnum.Percent.Name}.cs",
                ValueObjectEnum.Percent.Plural(),
                scaffoldingDirectoryStore.ProjectBaseName);
            
            return @$"namespace {classNamespace};

using {percentClassPath.ClassNamespace};
using FluentValidation;

// source: https://github.com/asc-lab/better-code-with-ddd/blob/ef_core/LoanApplication.TacticalDdd/LoanApplication.TacticalDdd/DomainModel/MonetaryAmount.cs
public class {ValueObjectEnum.MonetaryAmount.Name} : ValueObject
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
        }
        
        private string GetRoleFileText(string classNamespace)
        {
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(scaffoldingDirectoryStore.SrcDirectory, "", scaffoldingDirectoryStore.ProjectBaseName);
            
            return @$"namespace {classNamespace};

using {exceptionsClassPath.ClassNamespace};
using Ardalis.SmartEnum;

public class Role : ValueObject
{{
    private RoleEnum _role;
    public string Value
    {{
        get => _role.Name;
        private set
        {{
            if (!RoleEnum.TryFromName(value, true, out var parsed))
                throw new ValidationException($""Invalid Role. PLease use one of the following: {{string.Join("", "", ListNames())}}"");

            _role = parsed;
        }}
    }}
    
    public Role(string value)
    {{
        Value = value;
    }}
    
    public static Role Of(string value) => new Role(value);
    public static implicit operator string(Role value) => value.Value;
    public static List<string> ListNames() => RoleEnum.List.Select(x => x.Name).ToList();

    public static Role User() => new Role(RoleEnum.User.Name);
    public static Role SuperAdmin() => new Role(RoleEnum.SuperAdmin.Name);

    protected Role() {{ }} // EF Core

    private abstract class RoleEnum(string name, int value)
        : SmartEnum<RoleEnum>(name, value)
    {{
        public static readonly RoleEnum User = new UserType();
        public static readonly RoleEnum SuperAdmin = new SuperAdminType();

        private class UserType() : RoleEnum(""User"", 0);
        private class SuperAdminType() : RoleEnum(""Super Admin"", 1);
    }}
}}";
        }
        private string GetEmailFileText(string classNamespace)
        {

            return @$"namespace {classNamespace};

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
        }
    }
}
