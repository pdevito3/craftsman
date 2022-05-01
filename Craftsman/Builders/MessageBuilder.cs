namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using Domain;
using Helpers;
using Services;

public class MessageBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public MessageBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateMessage(string solutionDirectory, Message message)
    {
        var classPath = ClassPathHelper.MessagesClassPath(solutionDirectory, $"{FileNames.MessageClassName(message.Name)}.cs");
        var fileText = GetMessageFileText(classPath.ClassNamespace, message);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetMessageFileText(string classNamespace, Message message)
    {
        var propString = MessagePropBuilder(message.Properties);

        return @$"namespace {classNamespace}
{{
    using System;
    using System.Text;

    public interface {FileNames.MessageInterfaceName(message.Name)}
    {{
        {propString}
    }}

    public class {FileNames.MessageClassName(message.Name)} : {FileNames.MessageInterfaceName(message.Name)}
    {{
        {propString}
    }}
}}";
    }

    public static string MessagePropBuilder(List<MessageProperty> props)
    {
        var propString = "";
        for (var eachProp = 0; eachProp < props.Count; eachProp++)
        {
            string newLine = eachProp == props.Count - 1 ? "" : $"{Environment.NewLine}{Environment.NewLine}";
            propString += $@"public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{newLine}";
        }

        return propString;
    }
}
