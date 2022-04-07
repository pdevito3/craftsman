namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class DotRunBuilder
    {
        public static void BuildRunBoundariesXml(string solutionDirectory, string solutionName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.DotRunClassPath(solutionDirectory, $"{solutionName}Boundaries.run.xml");
            var fileText = BuildRunFile(solutionName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string BuildRunFile(string solutionName)
        {
            return @$"<component name=""ProjectRunConfigurationManager"">
  <configuration default=""false"" name=""{solutionName}Boundaries"" type=""CompoundRunConfigurationType"">
    <method v=""2"" />
  </configuration>
</component>";
        }
    }
}