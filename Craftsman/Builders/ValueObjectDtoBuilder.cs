namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class ValueObjectDtoBuilder
{
    public class ValueObjectDtoBuilderCommand : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<ValueObjectDtoBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(ValueObjectDtoBuilderCommand request, CancellationToken cancellationToken)
        {
            var addressReadDtoClassPath = ClassPathHelper.WebApiValueObjectDtosClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                ValueObjectEnum.Address,
                Dto.Read,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var readDtoText = GetAddressDtoText(addressReadDtoClassPath.ClassNamespace);
            _utilities.CreateFile(addressReadDtoClassPath, readDtoText);
            
            var addressCreateDtoClassPath = ClassPathHelper.WebApiValueObjectDtosClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                ValueObjectEnum.Address,
                Dto.Creation,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var createDtoText = GetAddressCreateDtoText(addressCreateDtoClassPath.ClassNamespace);
            _utilities.CreateFile(addressCreateDtoClassPath, createDtoText);
            
            var addressUpdateDtoClassPath = ClassPathHelper.WebApiValueObjectDtosClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                ValueObjectEnum.Address,
                Dto.Update,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var updateDtoText = GetAddressUpdateDtoText(addressUpdateDtoClassPath.ClassNamespace);
            _utilities.CreateFile(addressUpdateDtoClassPath, updateDtoText);

            return Task.FromResult(true);
        }
        
        private string GetAddressDtoText(string classNamespace)
        {
            var dtoName = FileNames.GetDtoName(ValueObjectEnum.Address.Name, Dto.Read);
            return $@"namespace {classNamespace};
            
public class {dtoName}
{{
    public string Line1 {{ get; set; }}
    public string Line2 {{ get; set; }}
    public string City {{ get; set; }}
    public string State {{ get; set; }}
    public string PostalCode {{ get; set; }}
    public string Country {{ get; set; }}
}}";
        }
        
        private string GetAddressCreateDtoText(string classNamespace)
        {
            var dtoName = FileNames.GetDtoName(ValueObjectEnum.Address.Name, Dto.Creation);
            return $@"namespace {classNamespace};
            
public class {dtoName}
{{
    public string Line1 {{ get; set; }}
    public string Line2 {{ get; set; }}
    public string City {{ get; set; }}
    public string State {{ get; set; }}
    public string PostalCode {{ get; set; }}
    public string Country {{ get; set; }}
}}";
        }
        
        private string GetAddressUpdateDtoText(string classNamespace)
        {
            var dtoName = FileNames.GetDtoName(ValueObjectEnum.Address.Name, Dto.Update);
            return $@"namespace {classNamespace};
            
public class {dtoName}
{{
    public string Line1 {{ get; set; }}
    public string Line2 {{ get; set; }}
    public string City {{ get; set; }}
    public string State {{ get; set; }}
    public string PostalCode {{ get; set; }}
    public string Country {{ get; set; }}
}}";
        }

    }
}
