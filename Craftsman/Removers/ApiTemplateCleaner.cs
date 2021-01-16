namespace Craftsman.Removers
{
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ApiTemplateCleaner
    {
        public static void CleanTemplateFilesAndDirectories(string solutionDirectory, string solutionName, string authMethod, List<ApplicationUser> applicationUsers)
        {
            var templEntityName = "ValueToReplace";
            EntityRemover.RemoveDirectory(solutionDirectory);
            IRepositoryRemover.RemoveDirectory(solutionDirectory, $"{templEntityName}");
            RepositoryRemover.RemoveDirectory(solutionDirectory);
            ProfileRemover.RemoveDirectory(solutionDirectory);
            DtoRemover.RemoveDirectory(solutionDirectory, $"{templEntityName}");
            ValidationRemover.RemoveDirectory(solutionDirectory, $"{templEntityName}");
            DbContextRemover.RemoveDirectory(solutionDirectory);
            SeederRemover.RemoveDirectory(solutionDirectory);
            ControllerRemover.RemoveDirectory(solutionDirectory);


            TestFakesRemover.RemoveDirectory(solutionDirectory, templEntityName, solutionName);
            TestRepositoryRemover.RemoveDirectory(solutionDirectory, templEntityName, solutionName);
            TestIntegrationRemover.RemoveDirectory(solutionDirectory, templEntityName, solutionName);

            InfrastructurePersistenceServicesCleaner.CleanServiceRegistration(solutionDirectory);
            StartupCleaner.CleanStartup(solutionDirectory, authMethod, applicationUsers);

            if (authMethod != "JWT")
            {
                IdentityRemover.RemoveProject(solutionDirectory);
                IdentityRemover.RemoveController(solutionDirectory);
                IdentityRemover.RemoveDtos(solutionDirectory);
                IdentityRemover.RemoveIAccountService(solutionDirectory);
                //IdentityRemover.RemoveAuditableEntity(solutionDirectory);
                IdentityRemover.RemoveJwtSettings(solutionDirectory);
                IdentityRemover.RemoveRoles(solutionDirectory);
                IdentityRemover.RemoveCurrentUserService(solutionDirectory);
                IdentityRemover.RemoveCurrentUserServiceInterface(solutionDirectory);
            }
        }
    }
}
