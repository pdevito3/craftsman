namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class CreatedDomainEventBuilder
{
    public class CreatedDomainEventBuilderCommand(string entityName, string entityPlural) : IRequest<bool>
    {
        public string EntityName { get; set; } = entityName;
        public string EntityPlural { get; set; } = entityPlural;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CreatedDomainEventBuilderCommand, bool>
    {
        public Task<bool> Handle(CreatedDomainEventBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.DomainEventsClassPath(scaffoldingDirectoryStore.SrcDirectory,
                $"{FileNames.EntityCreatedDomainMessage(request.EntityName)}.cs",
                request.EntityPlural,
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.EntityName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace, string entityName)
        {
            // lang=csharp
            return $$"""
                     namespace {{classNamespace}};

                     public sealed class {{FileNames.EntityCreatedDomainMessage(entityName)}} : DomainEvent
                     {
                         public {{entityName}} {{entityName}} { get; set; } 
                     }
                                 
                     """;
        }
    }
}