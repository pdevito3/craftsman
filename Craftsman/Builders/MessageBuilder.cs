namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using Domain;
using Helpers;
using MediatR;
using Services;

public static class MessageBuilder
{
    public sealed record MessageBuilderCommand(string SolutionDirectory, Message Message) : IRequest;

    public class Handler(ICraftsmanUtilities utilities) : IRequestHandler<MessageBuilderCommand>
    {
        public Task Handle(MessageBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.MessagesClassPath(request.SolutionDirectory, $"{FileNames.MessageClassName(request.Message.Name)}.cs");
            var fileText = GetMessageFileText(classPath.ClassNamespace, request.Message);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
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
}
