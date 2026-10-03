namespace Craftsman.Tests;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Craftsman.Domain;
using Craftsman.Helpers;
using FluentAssertions;
using Xunit;

public class FileParsingHelperTests
{
    private const string YamlWithBoundedContextMessages = """
        DomainName: WeSendReportsCompany
        BoundedContexts:
          - ProjectName: Reporting
            Producers:
              - EndpointRegistrationMethodName: SubmitReportRequest
                ExchangeName: report-requests
                MessageName: ISendReportRequest
                ExchangeType: fanout
                ProducerName: ReportWasRequested
                UsesDb: true
            Consumers:
              - EndpointRegistrationMethodName: AllReportsGetSentFromHereEndpoint
                ConsumerName: SenderOfAllReports
                ExchangeName: report-requests
                MessageName: ISendReportRequest
                QueueName: all-reports
                ExchangeType: fanout
            Messages:
              - Name: ISendReportRequest
                Properties:
                  - Name: ReportId
                    Type: guid
                  - Name: Provider
                    Type: string
                  - Name: Target
                    Type: string
            Bus:
              AddBus: true
        """;

    private const string YamlWithBothRootAndBoundedContextMessages = """
        DomainName: MultiMessageDomain
        Messages:
          - Name: IGlobalDomainMessage
            Properties:
              - Name: EventId
                Type: guid
        BoundedContexts:
          - ProjectName: Reporting
            Messages:
              - Name: ISendReportRequest
                Properties:
                  - Name: ReportId
                    Type: guid
        """;

    private const string YamlWithoutBoundedContextMessages = """
        DomainName: StandardDomain
        BoundedContexts:
          - ProjectName: Reporting
            Port: 5001
        """;

    private const string JsonWithBoundedContextMessages = """
        {
          "DomainName": "WeSendReportsCompany",
          "BoundedContexts": [
            {
              "ProjectName": "Reporting",
              "Messages": [
                {
                  "Name": "ISendReportRequest",
                  "Properties": [
                    {
                      "Name": "ReportId",
                      "Type": "guid"
                    },
                    {
                      "Name": "Provider",
                      "Type": "string"
                    },
                    {
                      "Name": "Target",
                      "Type": "string"
                    }
                  ]
                }
              ]
            }
          ]
        }
        """;

    [Fact]
    public void ReadYamlString_with_messages_in_bounded_context_deserializes_domain_project_successfully()
    {
        // Act
        var domainProject = FileParsingHelper.ReadYamlString<DomainProject>(YamlWithBoundedContextMessages);

        // Assert
        domainProject.Should().NotBeNull();
        domainProject.DomainName.Should().Be("WeSendReportsCompany");
        domainProject.BoundedContexts.Should().HaveCount(1);

        var messages = domainProject.BoundedContexts[0].Messages;
        messages.Should().NotBeNull();
        messages.Should().HaveCount(1);

        var message = messages[0];
        message.Name.Should().Be("SendReportRequest");
        message.Properties.Should().HaveCount(3);
        message.Properties[0].Name.Should().Be("ReportId");
        message.Properties[0].Type.Should().Be("Guid");
        message.Properties[1].Name.Should().Be("Provider");
        message.Properties[1].Type.Should().Be("string");
        message.Properties[2].Name.Should().Be("Target");
        message.Properties[2].Type.Should().Be("string");
    }

    [Fact]
    public void ReadYaml_with_messages_in_bounded_context_deserializes_domain_project_from_file_successfully()
    {
        // Arrange
        const string filePath = "/fake/template.yaml";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData(YamlWithBoundedContextMessages) }
        });
        var helper = new FileParsingHelper(fileSystem);

        // Act
        var domainProject = helper.ReadYaml<DomainProject>(filePath);

        // Assert
        domainProject.Should().NotBeNull();
        domainProject.DomainName.Should().Be("WeSendReportsCompany");
        domainProject.BoundedContexts.Should().HaveCount(1);

        var messages = domainProject.BoundedContexts[0].Messages;
        messages.Should().NotBeNull();
        messages.Should().HaveCount(1);
        messages[0].Name.Should().Be("SendReportRequest");
        messages[0].Properties.Should().HaveCount(3);
    }

    [Fact]
    public void ReadYamlString_with_messages_in_bounded_contexts_template_deserializes_successfully()
    {
        // Arrange
        const string yaml = """
            BoundedContexts:
              - ProjectName: Reporting
                Messages:
                  - Name: ISendReportRequest
                    Properties:
                      - Name: ReportId
                        Type: guid
            """;

        // Act
        var bcTemplate = FileParsingHelper.ReadYamlString<BoundedContextsTemplate>(yaml);

        // Assert
        bcTemplate.Should().NotBeNull();
        bcTemplate.BoundedContexts.Should().HaveCount(1);

        var messages = bcTemplate.BoundedContexts[0].Messages;
        messages.Should().NotBeNull();
        messages.Should().HaveCount(1);
        messages[0].Name.Should().Be("SendReportRequest");
        messages[0].Properties.Should().HaveCount(1);
        messages[0].Properties[0].Name.Should().Be("ReportId");
        messages[0].Properties[0].Type.Should().Be("Guid");
    }

    [Fact]
    public void ReadYamlString_with_both_root_and_bounded_context_messages_deserializes_both_levels_successfully()
    {
        // Act
        var domainProject = FileParsingHelper.ReadYamlString<DomainProject>(YamlWithBothRootAndBoundedContextMessages);

        // Assert
        domainProject.Should().NotBeNull();
        domainProject.DomainName.Should().Be("MultiMessageDomain");
        domainProject.Messages.Should().HaveCount(1);
        domainProject.Messages[0].Name.Should().Be("GlobalDomainMessage");

        domainProject.BoundedContexts.Should().HaveCount(1);
        var bcMessages = domainProject.BoundedContexts[0].Messages;
        bcMessages.Should().NotBeNull();
        bcMessages.Should().HaveCount(1);
        bcMessages[0].Name.Should().Be("SendReportRequest");
    }

    [Fact]
    public void ReadYamlString_without_bounded_context_messages_preserves_backward_compatibility()
    {
        // Act
        var domainProject = FileParsingHelper.ReadYamlString<DomainProject>(YamlWithoutBoundedContextMessages);

        // Assert
        domainProject.Should().NotBeNull();
        domainProject.DomainName.Should().Be("StandardDomain");
        domainProject.BoundedContexts.Should().HaveCount(1);
        domainProject.BoundedContexts[0].ProjectName.Should().Be("Reporting");
    }

    [Fact]
    public void ReadJson_with_messages_in_bounded_context_deserializes_domain_project_successfully()
    {
        // Arrange
        const string filePath = "/fake/template.json";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData(JsonWithBoundedContextMessages) }
        });
        var helper = new FileParsingHelper(fileSystem);

        // Act
        var domainProject = helper.ReadJson<DomainProject>(filePath);

        // Assert
        domainProject.Should().NotBeNull();
        domainProject.DomainName.Should().Be("WeSendReportsCompany");
        domainProject.BoundedContexts.Should().HaveCount(1);

        var messages = domainProject.BoundedContexts[0].Messages;
        messages.Should().NotBeNull();
        messages.Should().HaveCount(1);
        messages[0].Name.Should().Be("SendReportRequest");
        messages[0].Properties.Should().HaveCount(3);
        messages[0].Properties[0].Name.Should().Be("ReportId");
        messages[0].Properties[0].Type.Should().Be("Guid");
    }
}
