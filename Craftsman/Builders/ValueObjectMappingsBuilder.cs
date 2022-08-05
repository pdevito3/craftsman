namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class ValueObjectMappingsBuilder
{
    public class ValueObjectMappingsBuilderCommand : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<ValueObjectMappingsBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(ValueObjectMappingsBuilderCommand request, CancellationToken cancellationToken)
        {
            var percentClassPath = ClassPathHelper.WebApiValueObjectMappingsClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                ValueObjectEnum.Percent,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var percentFileText = GetPercentFileText(percentClassPath.ClassNamespace);
            _utilities.CreateFile(percentClassPath, percentFileText);
            
            var addressClassPath = ClassPathHelper.WebApiValueObjectMappingsClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                ValueObjectEnum.Address,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var addressFileText = GetAddressFileText(addressClassPath.ClassNamespace);
            _utilities.CreateFile(addressClassPath, addressFileText);
            
            var monetaryAmountClassPath = ClassPathHelper.WebApiValueObjectMappingsClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                ValueObjectEnum.MonetaryAmount,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var monetaryAmountFileText = GetMonetaryAmountFileText(monetaryAmountClassPath.ClassNamespace);
            _utilities.CreateFile(monetaryAmountClassPath, monetaryAmountFileText);

            return Task.FromResult(true);
        }
        
        private string GetPercentFileText(string classNamespace)
        {
            var mappingName = FileNames.GetMappingName(ValueObjectEnum.Percent.Name);
            var voClassPath = ClassPathHelper.SharedKernelDomainClassPath(_scaffoldingDirectoryStore.SolutionDirectory, "");
            
            return @$"namespace {classNamespace};

using {voClassPath.ClassNamespace};
using Mapster;

public class {mappingName} : IRegister
{{
    public void Register(TypeAdapterConfig config)
    {{
        config.NewConfig<decimal, Percent>()
            .MapWith(value => new Percent(value));
        config.NewConfig<Percent, decimal>()
            .MapWith(percent => percent.Value);
    }}
}}";
        }
        
        private string GetAddressFileText(string classNamespace)
        {
            var mappingName = FileNames.GetMappingName(ValueObjectEnum.Address.Name);
            var voClassPath = ClassPathHelper.SharedKernelDomainClassPath(_scaffoldingDirectoryStore.SolutionDirectory, "");
            var dtoClassPath = ClassPathHelper.WebApiValueObjectDtosClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"",
                ValueObjectEnum.Address.Plural(),
                _scaffoldingDirectoryStore.ProjectBaseName);
            
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {voClassPath.ClassNamespace};
using Mapster;

public class {mappingName} : IRegister
{{
    public void Register(TypeAdapterConfig config)
    {{
        config.NewConfig<string, PostalCode>()
            .MapWith(value => new PostalCode(value));
        config.NewConfig<PostalCode, string>()
            .MapWith(postalCode => postalCode.Value);

        config.NewConfig<AddressDto, Address>()
            .MapWith(address => new Address(address.Line1, address.Line2, address.City, address.State, address.PostalCode, address.Country))
            .TwoWays();
        config.NewConfig<AddressForCreationDto, Address>()
            .MapWith(address => new Address(address.Line1, address.Line2, address.City, address.State, address.PostalCode, address.Country))
            .TwoWays();
        config.NewConfig<AddressForUpdateDto, Address>()
            .MapWith(address => new Address(address.Line1, address.Line2, address.City, address.State, address.PostalCode, address.Country))
            .TwoWays();
    }}
}}";
        }
        
        private string GetMonetaryAmountFileText(string classNamespace)
        {
            var mappingName = FileNames.GetMappingName(ValueObjectEnum.MonetaryAmount.Name);
            var voClassPath = ClassPathHelper.SharedKernelDomainClassPath(_scaffoldingDirectoryStore.SolutionDirectory, "");
            
            return @$"namespace {classNamespace};

using {voClassPath.ClassNamespace};
using Mapster;

public class {mappingName} : IRegister
{{
    public void Register(TypeAdapterConfig config)
    {{
        config.NewConfig<decimal, MonetaryAmount>()
            .MapWith(value => new MonetaryAmount(value));
        config.NewConfig<MonetaryAmount, decimal>()
            .MapWith(monetaryAmount => monetaryAmount.Amount);
    }}
}}";
        }
    }
}
