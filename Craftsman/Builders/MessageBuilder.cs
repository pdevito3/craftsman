namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public static class MessageBuilder
    {
        public static void CreateMessage(string solutionDirectory, Message message, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.MessagesProjectRootClassPath(solutionDirectory, $"{message.Name}.cs");

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using var fs = fileSystem.File.Create(classPath.FullClassPath);
            var data = GetMessageFileText(classPath.ClassNamespace, message);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetMessageFileText(string classNamespace, Message message)
        {
            var propString = MessagePropBuilder(message.Properties);

            return @$"namespace {classNamespace}
{{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface {message.Name}
    {{
{propString}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";
        }

        public static string MessagePropBuilder(List<MessageProperty> props)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                string newLine = eachProp == props.Count - 1 ? "" : $"{Environment.NewLine}{Environment.NewLine}";
                propString += $@"        public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }
    }
}