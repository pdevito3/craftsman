namespace Craftsman.Removers
{
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ApiTemplateCleaner
    {
        public static void CleanTemplateFilesAndDirectories(string solutionDirectory, ApiTemplate template)
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

            TestFakesRemover.RemoveDirectory(solutionDirectory, templEntityName, template);
            TestRepositoryRemover.RemoveDirectory(solutionDirectory, templEntityName, template);
            TestIntegrationRemover.RemoveDirectory(solutionDirectory, templEntityName, template);

            InfrastructurePersistenceServicesCleaner.CleanServiceRegistration(solutionDirectory);
            StartupCleaner.CleanStartup(solutionDirectory, template);

            if(template.AuthSetup.AuthMethod != "JWT")
            {
                IdentityRemover.RemoveProject(solutionDirectory);
                IdentityRemover.RemoveController(solutionDirectory);
                IdentityRemover.RemoveDtos(solutionDirectory);
                IdentityRemover.RemoveIAccountService(solutionDirectory);
                IdentityRemover.RemoveAuditableEntity(solutionDirectory);
                IdentityRemover.RemoveJwtSettings(solutionDirectory);
                IdentityRemover.RemoveRoles(solutionDirectory);
            }
        }
    }
}
