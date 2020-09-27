namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public class IdentityRemover
    {
        public static void RemoveProject(string solutionDirectory)
        {
            RemoveProjectFromSolution(solutionDirectory);
            RemoveProjectReferences(solutionDirectory);

            var classPath = ClassPathHelper.IdentityProjectPath(solutionDirectory); // deleting directory, so I don't need to give a meaningful filename

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            var dir = new DirectoryInfo(classPath.ClassDirectory);
            dir.Delete(true);
        }

        public static void RemoveController(string solutionDirectory)
        {
            var classPath = ClassPathHelper.ControllerClassPath(solutionDirectory, "AuthController.cs", "");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }

        public static void RemoveDtos(string solutionDirectory)
        {
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", "Auth");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            var dir = new DirectoryInfo(classPath.ClassDirectory);
            dir.Delete(true);
        }

        public static void RemoveIAccountService(string solutionDirectory)
        {
            var classPath = ClassPathHelper.ApplicationInterfaceClassPath(solutionDirectory, "IAccountService.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }

        public static void RemoveAuditableEntity(string solutionDirectory)
        {
            var classPath = ClassPathHelper.CommonDomainClassPath(solutionDirectory, "AuditableEntity.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }

        public static void RemoveCurrentUserService(string solutionDirectory)
        {
            var classPath = ClassPathHelper.WebApiServicesClassPath(solutionDirectory, "CurrentUserService.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }

        public static void RemoveCurrentUserServiceInterface(string solutionDirectory)
        {
            var classPath = ClassPathHelper.ApplicationInterfaceClassPath(solutionDirectory, "ICurrentUserService.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }

        public static void RemoveRoles(string solutionDirectory)
        {
            var classPath = ClassPathHelper.DomainEnumClassPath(solutionDirectory, "Role.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }

        public static void RemoveJwtSettings(string solutionDirectory)
        {
            var classPath = ClassPathHelper.DomainSettingsClassPath(solutionDirectory, "JwtSettings.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }

        private static void RemoveProjectFromSolution(string solutionDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = @$"sln remove {Path.Combine("Infrastructure.Identity", "Infrastructure.Identity.csproj")}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = solutionDirectory
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static void RemoveProjectReferences(string solutionDirectory)
        {
            var projectToRemove = Path.Combine("..", "Infrastructure.Identity", "Infrastructure.Identity.csproj");
            var webApiProject = Path.Combine("WebApi", "WebApi.csproj");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = @$"remove {webApiProject} reference {projectToRemove}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = solutionDirectory
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
