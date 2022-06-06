namespace Craftsman.Builders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class SmartEnumBuilder
{
    public class SmartEnumBuilderCommand : IRequest<bool>
    {
        public readonly EntityProperty EntityProperty;
        public readonly string EntityPlural;

        public SmartEnumBuilderCommand(EntityProperty entityProperty, string entityPlural)
        {
            EntityProperty = entityProperty;
            EntityPlural = entityPlural;
        }
    }

    public class Handler : IRequestHandler<SmartEnumBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(SmartEnumBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.EntityClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{request.EntityProperty.SmartEnumPropName}.cs", 
                request.EntityPlural, 
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.EntityProperty);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace, EntityProperty entityProperty)
        {
            var types = "";
            var initializers = "";
            
            foreach (var smartOption in entityProperty.GetSmartOptions())
            {
                var despacedPropName = smartOption.Name.Replace(" ", "");

                types += $@"
    public static readonly {entityProperty.SmartEnumPropName} {despacedPropName} = new {despacedPropName}Type();";
                
                initializers += $@"

    private class {despacedPropName}Type : {entityProperty.SmartEnumPropName}
    {{
        public {despacedPropName}Type() : base(""{smartOption.Name}"", {smartOption.Value})
        {{
        }}
    }}";
            }
            
            
            return @$"namespace {classNamespace};

using Ardalis.SmartEnum;

public abstract class {entityProperty.SmartEnumPropName} : SmartEnum<{entityProperty.SmartEnumPropName}>
{{{types}

    protected {entityProperty.SmartEnumPropName}(string name, int value) : base(name, value)
    {{
    }}{initializers}
}}";
        }
    }
}
