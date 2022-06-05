namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class IBoundaryServiceInterfaceBuilder
{
    public class IBoundaryServiceInterfaceBuilderCommand : IRequest<bool>
    {
        public IBoundaryServiceInterfaceBuilderCommand()
        {
        }
    }

    public class Handler : IRequestHandler<IBoundaryServiceInterfaceBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(IBoundaryServiceInterfaceBuilderCommand request, CancellationToken cancellationToken)
        {
            var boundaryServiceName = FileNames.BoundaryServiceInterface(_scaffoldingDirectoryStore.ProjectBaseName);
            var classPath = ClassPathHelper.WebApiServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory, $"{boundaryServiceName}.cs", _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace)
        {
            var boundaryServiceName = FileNames.BoundaryServiceInterface(_scaffoldingDirectoryStore.ProjectBaseName);
            
            return @$"namespace {classNamespace};

public interface {boundaryServiceName}
{{
}}";
        }
    }
}
