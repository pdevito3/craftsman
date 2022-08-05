namespace Craftsman.Builders.Features;

using System;
using Domain;
using Helpers;
using Services;

public class ConsumerBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ConsumerBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateConsumerFeature(string solutionDirectory, string srcDirectory, Consumer consumer, string projectBaseName)
    {
        var classPath = ClassPathHelper.ConsumerFeaturesClassPath(srcDirectory, $"{consumer.ConsumerName}.cs", consumer.DomainDirectory, projectBaseName);
        var fileText = GetDirectOrTopicConsumerRegistration(classPath.ClassNamespace, consumer, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public string GetDirectOrTopicConsumerRegistration(string classNamespace, Consumer consumer, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var context = _utilities.GetDbContext(srcDirectory, projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var dbReadOnly = consumer.UsesDb ? @$"{Environment.NewLine}    private readonly {context} _db;" : "";
        var dbProp = consumer.UsesDb ? @$"{context} db, " : "";
        var assignDb = consumer.UsesDb ? @$"{Environment.NewLine}        _db = db;" : "";
        var contextUsing = consumer.UsesDb ? $@"
using {contextClassPath.ClassNamespace};" : "";

        var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");
        return @$"namespace {classNamespace};

using MapsterMapper;
using MassTransit;
using {messagesClassPath.ClassNamespace};
using System.Threading.Tasks;{contextUsing}

public class {consumer.ConsumerName} : IConsumer<{FileNames.MessageInterfaceName(consumer.MessageName)}>
{{
    private readonly IMapper _mapper;{dbReadOnly}

    public {consumer.ConsumerName}({dbProp}IMapper mapper)
    {{
        _mapper = mapper;{assignDb}
    }}

    public Task Consume(ConsumeContext<{FileNames.MessageInterfaceName(consumer.MessageName)}> context)
    {{
        // do work here

        return Task.CompletedTask;
    }}
}}";
    }
}
