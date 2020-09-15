namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class IdentityRoleBuilder
    {
        public static void CreateRoles(string solutionDirectory, List<string> roles)
        {
            try
            {
                var classPath = ClassPathHelper.DomainEnumClassPath(solutionDirectory, $"Role.cs");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetRoleFileText(classPath.ClassNamespace, roles);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetRoleFileText(string classNamespace, List<string> roles)
        {
            return @$"namespace {classNamespace}
{{
    public enum Role
    {{
        {string.Join($",{Environment.NewLine}",roles)}
    }}
}}";
        }
    }
}
