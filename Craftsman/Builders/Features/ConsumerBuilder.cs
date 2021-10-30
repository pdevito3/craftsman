namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public class ConsumerBuilder
    {
        public static void CreateConsumerFeature(string srcDirectory, Consumer consumer, string projectBaseName)
        {
            var classPath = ClassPathHelper.ConsumerFeaturesClassPath(srcDirectory, $"{consumer.ConsumerName}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = GetDirectOrTopicConsumerRegistration(classPath.ClassNamespace, consumer, srcDirectory, projectBaseName);

            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetDirectOrTopicConsumerRegistration(string classNamespace, Consumer consumer, string srcDirectory, string projectBaseName)
        {
            var context = Utilities.GetDbContext(srcDirectory, projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var dbReadOnly = consumer.UsesDb ? @$"{Environment.NewLine}        private readonly {context} _db;" : "";
            var dbProp = consumer.UsesDb ? @$"{context} db, " : "";
            var assignDb = consumer.UsesDb ? @$"{Environment.NewLine}            _db = db;" : "";
            var contextUsing = consumer.UsesDb ? $@"
    using {contextClassPath.ClassNamespace};" : "";

            return @$"namespace {classNamespace};

using AutoMapper;
using MassTransit;
using Messages;
using System.Threading.Tasks;{contextUsing}

public class {consumer.ConsumerName} : IConsumer<{consumer.MessageName}>
{{
    private readonly IMapper _mapper;{dbReadOnly}

    public {consumer.ConsumerName}({dbProp}IMapper mapper)
    {{
        _mapper = mapper;{assignDb}
    }}

    public class {consumer.ConsumerName}Profile : Profile
    {{
        public {consumer.ConsumerName}Profile()
        {{
            //createmap<to this, from this>
        }}
    }}

    public Task Consume(ConsumeContext<{consumer.MessageName}> context)
    {{
        // do work here

        return Task.CompletedTask;
    }}
}}";
        }
    }
}