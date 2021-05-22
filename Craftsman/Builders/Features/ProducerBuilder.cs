namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public class ProducerBuilder
    {
        public static void CreateProducerFeature(string srcDirectory, Producer producer, string projectBaseName)
        {
            var classPath = ClassPathHelper.ProducerFeaturesClassPath(srcDirectory, $"{producer.ProducerName}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = GetProducerRegistration(classPath.ClassNamespace, producer, srcDirectory, projectBaseName);

            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetProducerRegistration(string classNamespace, Producer producer, string srcDirectory, string projectBaseName)
        {
            var context = Utilities.GetDbContext(srcDirectory, projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var dbReadOnly = producer.UsesDb ? @$"{Environment.NewLine}            private readonly {context} _db;" : "";
            var dbProp = producer.UsesDb ? @$"{context} db, " : "";
            var assignDb = producer.UsesDb ? @$"{Environment.NewLine}                _db = db;" : "";
            var contextUsing = producer.UsesDb ? $@"
    using {contextClassPath.ClassNamespace};" : "";

            var commandProp = "SomeContentToPutInMessage";
            var commandPropLower = commandProp.LowercaseFirstLetter();
            var propTypeToReturn = "bool";
            var commandName = $"{producer.ProducerName}Command";

            return @$"namespace {classNamespace}
{{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using MassTransit;
    using Messages;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;{contextUsing}

    public static class {producer.ProducerName}
    {{
        public class {commandName} : IRequest<{propTypeToReturn}>
        {{
            public string {commandProp} {{ get; set; }}

            public {commandName}(string {commandPropLower})
            {{
                {commandProp} = {commandPropLower};
            }}
        }}

        public class Handler : IRequestHandler<{commandName}, {propTypeToReturn}>
        {{
            private readonly IPublishEndpoint _publishEndpoint;
            private readonly IMapper _mapper;{dbReadOnly}

            public Handler({dbProp}IMapper mapper, IPublishEndpoint publishEndpoint)
            {{
                _publishEndpoint = publishEndpoint;
                _mapper = mapper;{assignDb}
            }}

            public class {commandName}Profile : Profile
            {{
                public {commandName}Profile()
                {{
                    //createmap<to this, from this>
                }}
            }}

            public async Task<{propTypeToReturn}> Handle({commandName} request, CancellationToken cancellationToken)
            {{
                var message = new
                {{
                    // map content to message here or with automapper
                }};
                await _publishEndpoint.Publish<{producer.MessageName}>(message);

                return true;
            }}
        }}
    }}
}}";
        }
    }
}