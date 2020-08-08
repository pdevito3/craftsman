namespace Craftsman.Removers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ApiTemplateCleaner
    {
        public static void CleanTemplateFilesAndDirectories(string solutionDirectory)
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

            InfrastructurePersistenceServicesCleaner.CleanServiceRegistration(solutionDirectory);
            StartupCleaner.CleanStartup(solutionDirectory);
        }
    }
}
