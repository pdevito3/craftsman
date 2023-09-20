namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class HangfireAuthorizationFilterBuilder
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
                $"HangfireAuthorizationFilter.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using Hangfire.Dashboard;

public class HangfireAuthorizationFilter : IDashboardAsyncAuthorizationFilter
{{
    private readonly IServiceProvider _serviceProvider;
    
    public HangfireAuthorizationFilter(IServiceProvider serviceProvider)
    {{
        _serviceProvider = serviceProvider;
    }}

    public Task<bool> AuthorizeAsync(DashboardContext context)
    {{
        // TODO alt -- add login handling with cookie handling
        // var heimGuard = _serviceProvider.GetService<IHeimGuardClient>();
        // return await heimGuard.HasPermissionAsync(Permissions.HangfireAccess);

        var env = _serviceProvider.GetService<IWebHostEnvironment>();
        return Task.FromResult(env.IsDevelopment());
    }}
}}";
        }
    }
}