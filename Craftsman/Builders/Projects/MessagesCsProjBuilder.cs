namespace Craftsman.Builders.Projects
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class MessagesCsProjBuilder
    {
        public static void CreateMessagesCsProj(string solutionDirectory)
        {
            var classPath = ClassPathHelper.MessagesProjectClassPath(solutionDirectory);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetMessagesCsProjFileText();
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetMessagesCsProjFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

</Project>";
        }
    }
}