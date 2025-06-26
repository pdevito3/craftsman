namespace Craftsman.Builders.Dtos;

using Helpers;
using MediatR;
using Services;

public static class BasePaginationParametersBuilder
{
    public class BasePaginationParametersBuilderCommand : IRequest<bool>
    {
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<BasePaginationParametersBuilderCommand, bool>
    {
        public Task<bool> Handle(BasePaginationParametersBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiResourcesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"BasePaginationParameters.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetBasePaginationParametersText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetBasePaginationParametersText(string classNamespace)
        {
            // lang=csharp
            return $$"""
                     namespace {{classNamespace}};
                     public abstract class BasePaginationParameters
                     {
                         internal virtual int MaxPageSize { get; } = 500;
                         internal virtual int DefaultPageSize { get; set; } = 10;

                         public virtual int PageNumber { get; set; } = 1;

                         public int PageSize
                         {
                             get
                             {
                                 return DefaultPageSize;
                             }
                             set
                             {
                                 DefaultPageSize = value > MaxPageSize ? MaxPageSize : value;
                             }
                         }
                     }
                     """;
        }
    }
}
