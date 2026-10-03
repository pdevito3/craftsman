namespace Craftsman.Tests;

using System.Collections.Generic;
using Craftsman.Domain;
using Craftsman.Exceptions;
using FluentAssertions;
using Xunit;

public class MessageTests
{
    [Theory]
    [InlineData("ISendReportRequest", "SendReportRequest")]
    [InlineData("SendReportRequest", "SendReportRequest")]
    [InlineData("sendReportRequest", "SendReportRequest")]
    [InlineData("ItemCreated", "ItemCreated")]
    public void Name_removes_interface_prefix_and_uppercases_first_letter(string input, string expected)
    {
        var message = new Message { Name = input };

        message.Name.Should().Be(expected);
    }

    [Fact]
    public void Name_is_null_when_not_set()
    {
        var message = new Message();

        message.Name.Should().BeNull();
    }

    [Fact]
    public void MergeByName_collapses_identical_declarations_case_insensitively()
    {
        var messages = new List<Message>
        {
            NewMessage("ISendReportRequest", ("ReportId", "guid")),
            NewMessage("sendReportRequest", ("ReportId", "guid")),
            NewMessage("IOtherMessage"),
        };

        var merged = Message.MergeByName(messages);

        merged.Should().HaveCount(2);
        merged[0].Name.Should().Be("SendReportRequest");
        merged[1].Name.Should().Be("OtherMessage");
    }

    [Fact]
    public void MergeByName_skips_null_and_unnamed_messages()
    {
        var messages = new List<Message> { null!, new Message(), NewMessage("ISendReportRequest") };

        var merged = Message.MergeByName(messages);

        merged.Should().ContainSingle().Which.Name.Should().Be("SendReportRequest");
    }

    [Fact]
    public void MergeByName_throws_when_declarations_with_same_name_have_different_properties()
    {
        var messages = new List<Message>
        {
            NewMessage("ISendReportRequest", ("ReportId", "guid")),
            NewMessage("ISendReportRequest", ("ReportId", "string")),
        };

        var act = () => Message.MergeByName(messages);

        act.Should().Throw<InvalidTemplateException>().WithMessage("*`SendReportRequest`*");
    }

    private static Message NewMessage(string name, params (string Name, string Type)[] properties)
    {
        var message = new Message { Name = name };
        foreach (var (propName, propType) in properties)
            message.Properties.Add(new MessageProperty { Name = propName, Type = propType });
        return message;
    }
}
