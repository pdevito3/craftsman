namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class CurrentUserServiceBuilder
{
    public sealed record CurrentUserServiceBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<CurrentUserServiceBuilderCommand>
    {
        public Task Handle(CurrentUserServiceBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServicesClassPath(scaffoldingDirectoryStore.SrcDirectory, "CurrentUserService.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCurrentUserServiceText(classPath.ClassNamespace, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private static string GetCurrentUserServiceText(string classNamespace, string srcDirectory, string projectBaseName)
        {
            var boundaryServiceName = FileNames.BoundaryServiceInterface(projectBaseName);
            var hangfireUtilsClassPath = ClassPathHelper.HangfireResourcesClassPath(srcDirectory, $"", projectBaseName);

            // lang=csharp
            return $$"""
                     namespace {{classNamespace}};

                     using System.Security.Claims;
                     using {{hangfireUtilsClassPath.ClassNamespace}};

                     public interface ICurrentUserService : {{boundaryServiceName}}
                     {
                         ClaimsPrincipal? User { get; }
                         string? UserId { get; }
                         string? Email { get; }
                         string? FirstName { get; }
                         string? LastName { get; }
                         string? Username { get; }
                         string? ClientId { get; }
                         bool IsMachine { get; }
                     }

                     public sealed class CurrentUserService : ICurrentUserService
                     {
                         private readonly IHttpContextAccessor _httpContextAccessor;
                         private readonly IJobContextAccessor _jobContextAccessor;

                         public CurrentUserService(IHttpContextAccessor httpContextAccessor, IJobContextAccessor jobContextAccessor)
                         {
                             _httpContextAccessor = httpContextAccessor;
                             _jobContextAccessor = jobContextAccessor;
                         }

                         public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User ?? CreatePrincipalFromJobContextUserId();
                         public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
                         public string? Email => User?.FindFirstValue(ClaimTypes.Email);
                         public string? FirstName => User?.FindFirstValue(ClaimTypes.GivenName);
                         public string? LastName => User?.FindFirstValue(ClaimTypes.Surname);
                         public string? Username => User
                             ?.Claims
                             ?.FirstOrDefault(x => x.Type is "preferred_username" or "username")
                             ?.Value;
                         public string? ClientId => User
                             ?.Claims
                             ?.FirstOrDefault(x => x.Type is "client_id" or "clientId")
                             ?.Value;
                         public bool IsMachine => ClientId != null;
                         
                         private ClaimsPrincipal? CreatePrincipalFromJobContextUserId()
                         {
                             var userId = _jobContextAccessor?.UserContext?.User;
                             if (string.IsNullOrEmpty(userId))
                             {
                                 return null;
                             }

                             var claims = new[]
                             {
                                 new Claim(ClaimTypes.NameIdentifier, userId)
                             };

                             var identity = new ClaimsIdentity(claims, $"hangfire-job-{userId}");
                             return new ClaimsPrincipal(identity);
                         }
                     }
                     """;
        }
    }
}
