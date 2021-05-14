namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class BaseMessageBuilder
    {
        public static void CreateBaseMessage(string solutionDirectory)
        {
            var classPath = ClassPathHelper.BaseMessageClassPath(solutionDirectory, $"{Utilities.GetBaseMessageName()}.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = "";
            data = GetBaseMessageFileText(classPath.ClassNamespace);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetBaseMessageFileText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System;

    public interface {Utilities.GetBaseMessageName()}
    {{
        public Guid CorrelationId {{ get; set; }}
    }}
}}";
        }
    }
}