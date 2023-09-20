namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class ServiceJobActivatorScopeBuilder
{
    public record Command() : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.HangfireResourcesClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"ServiceJobActivatorScope.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using Hangfire;
using Hangfire.Annotations;

public class ServiceJobActivatorScope : JobActivatorScope
{{
    private readonly IServiceScope _serviceScope;

    public ServiceJobActivatorScope([NotNull] IServiceScope serviceScope)
    {{
        _serviceScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
    }}

    public override object Resolve(Type type)
    {{
        return ActivatorUtilities.GetServiceOrCreateInstance(_serviceScope.ServiceProvider, type);
    }}

    public override void DisposeScope()
    {{
        _serviceScope.Dispose();
    }}
}}";
        }
    }
}