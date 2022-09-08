namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class UpdatedUserRoleDomainEventBuilder
{
    public class Command : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.DomainEventsClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"{FileNames.UserRolesUpdateDomainMessage()}.cs",
                "Users",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

public class {FileNames.UserRolesUpdateDomainMessage()} : DomainEvent
{{
    public Guid UserId;
}}
            ";
        }
    }
}