namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class CreatedDomainEventBuilder
{
    public class CreatedDomainEventBuilderCommand : IRequest<bool>
    {
        public string EntityName { get; set; }
        public string EntityPlural { get; set; }
        
        public CreatedDomainEventBuilderCommand(string entityName, string entityPlural)
        {
            EntityName = entityName;
            EntityPlural = entityPlural;
        }
    }

    public class Handler : IRequestHandler<CreatedDomainEventBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(CreatedDomainEventBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.DomainEventsClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"{FileNames.EntityCreatedDomainMessage(request.EntityName)}.cs",
                request.EntityPlural,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.EntityName, request.EntityPlural);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace, string entityName, string entityPlural)
        {
            return @$"namespace {classNamespace};

public class {FileNames.EntityCreatedDomainMessage(entityPlural)} : IDomainEvent
{{
    public {entityName} {entityName} {{ get; set; }} 
}}
            ";
        }
    }
}