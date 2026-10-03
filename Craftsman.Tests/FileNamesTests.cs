namespace Craftsman.Tests;

using Craftsman.Services;
using FluentAssertions;
using Xunit;

public class FileNamesTests
{
    [Theory]
    [InlineData("ISendReportRequest", "SendReportRequest", "ISendReportRequest")]
    [InlineData("SendReportRequest", "SendReportRequest", "ISendReportRequest")]
    [InlineData("sendReportRequest", "SendReportRequest", "ISendReportRequest")]
    [InlineData("ItemCreated", "ItemCreated", "IItemCreated")]
    public void Message_class_and_interface_names_stay_in_sync(string messageName, string expectedClass, string expectedInterface)
    {
        FileNames.MessageClassName(messageName).Should().Be(expectedClass);
        FileNames.MessageInterfaceName(messageName).Should().Be(expectedInterface);
    }
}
