namespace Craftsman.Builders.Features;

using Craftsman.Domain;
using Craftsman.Helpers;
using Craftsman.Services;
using Humanizer;
using MediatR;

public static class JobFeatureBuilder
{
    public record Command(Feature Feature, string EntityPlural) : IRequest;

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
            var classPath = ClassPathHelper.FeaturesClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"{request.Feature.Name}.cs",
                request.EntityPlural,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.Feature);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace, Feature feature)
        {
            var hangfireUtilsClassPath = ClassPathHelper.HangfireResourcesClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var servicesUtilsClassPath = ClassPathHelper.WebApiServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"",
                _scaffoldingDirectoryStore.ProjectBaseName);
            
            return @$"namespace {classNamespace};

using Hangfire;
using HeimGuard;
using {hangfireUtilsClassPath.ClassNamespace};
using {servicesUtilsClassPath.ClassNamespace};

public class {feature.Name}
{{
    private readonly IUnitOfWork _unitOfWork;

    public {feature.Name}(IUnitOfWork unitOfWork)
    {{
        _unitOfWork = unitOfWork;
    }}
    
    public sealed class Command : IJobWithUserContext
    {{
        public string User {{ get; set; }}
    }}

    [JobDisplayName(""{feature.Name.Humanize(LetterCasing.Title)}"")]
    [AutomaticRetry(Attempts = 1)]
    // [Queue(Consts.HangfireQueues.{feature.Name.Humanize(LetterCasing.Title).Replace(" ", "")})]
    [CurrentUserFilter]
    public async Task Handle(Command command, CancellationToken cancellationToken)
    {{
        // TODO some work here
        await _unitOfWork.CommitChanges(cancellationToken);
    }}
}}";
        }
    }
}