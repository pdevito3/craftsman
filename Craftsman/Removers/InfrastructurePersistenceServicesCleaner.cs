namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class InfrastructurePersistenceServicesCleaner
    {
        public static void CleanServiceRegistration(string solutionDirectory)
        {
            var classPath = ClassPathHelper.InfraPersistenceServiceProviderClassPath(solutionDirectory, "ServiceRegistration.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";

            /* there are two regions in this file that need to be cleared. 
             * so we need to loop through each line in the file and, when we hit the start of the region, we 
             * need to skip every line after that (effectively deleting it because it won't end up in the new 
             * file that we are creating). We will keep skipping each line in the region until we hit the end 
             * of the region. this variable is going to track if we are in teh region and need to skip the current line
             */
            var removeLineSection = false;

            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";

                        // repo region, dbcontext region, using for interfaces
                        if (line.Contains("#region Repositories") || line.Contains("#region DbContext"))
                        {
                            output.WriteLine(newText);
                            removeLineSection = true;
                        }
                        else if (line.Contains("#endregion"))
                        {
                            output.WriteLine(newText);
                            removeLineSection = false;
                        }
                        else if (line.Contains("ValueToReplace"))
                        {
                            // do nothing so we skip it, but don't want to 
                            // update removeLineSection since it's just one line
                        }
                        else
                        {
                            if(!removeLineSection)
                                output.WriteLine(newText);
                        }

                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
