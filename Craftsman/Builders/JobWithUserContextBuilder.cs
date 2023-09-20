namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class JobWithUserContextBuilder
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
                $"JobWithUserContext.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using System.Security.Claims;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.AspNetCore;
using Hangfire.Client;
using Hangfire.Common;
using Services;

public interface IJobWithUserContext
{{
    public string? User {{ get; set; }}
}}
public class JobWithUserContext : IJobWithUserContext
{{
    public string? User {{ get; set; }}
}}
public interface IJobContextAccessor
{{
    JobWithUserContext? UserContext {{ get; set; }}
}}
public class JobContextAccessor : IJobContextAccessor
{{
    public JobWithUserContext? UserContext {{ get; set; }}
}}

public class JobWithUserContextActivator : AspNetCoreJobActivator
{{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public JobWithUserContextActivator([NotNull] IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
    {{
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }}

    public override JobActivatorScope BeginScope(JobActivatorContext context)
    {{
        var user = context.GetJobParameter<string>(""User"");

        if (user == null)
        {{
            return base.BeginScope(context);
        }}

        var serviceScope = _serviceScopeFactory.CreateScope();

        var userContextForJob = serviceScope.ServiceProvider.GetRequiredService<IJobContextAccessor>();
        userContextForJob.UserContext = new JobWithUserContext {{User = user}};

        return new ServiceJobActivatorScope(serviceScope);
    }}
}}";
        }
    }
}