namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandRemoveUserRoleBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandRemoveUserRoleBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.RemoveUserRoleFeatureClassName()}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName)
    {
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");

        return @$"namespace {classNamespace};

using {entityServicesClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using HeimGuard;
using MediatR;
using Roles;

public static class {FileNames.RemoveUserRoleFeatureClassName()}
{{
    public class Command : IRequest<bool>
    {{
        public readonly Guid UserId;
        public readonly string Role;

        public Command(Guid userId, string role)
        {{
            UserId = userId;
            Role = role;
        }}
    }}

    public class Handler : IRequestHandler<Command, bool>
    {{
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {{
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }}

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {{
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanRemoveUserRole);
            var user = await _userRepository.GetById(request.UserId, true, cancellationToken);

            var roleToRemove = user.RemoveRole(new Role(request.Role));
            _userRepository.RemoveRole(roleToRemove);
            _userRepository.Update(user);
            await _unitOfWork.CommitChanges(cancellationToken);

            return true;
        }}
    }}
}}";
    }
}
