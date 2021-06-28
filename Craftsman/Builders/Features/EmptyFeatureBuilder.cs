namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public class EmptyFeatureBuilder
    {
        public static void CreateCommand(string srcDirectory, string contextName, string projectBaseName, NewFeatureProperties featureProperties)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{featureProperties.Feature}.cs", featureProperties.Directory, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = "";
            data = GetCommandFileText(classPath.ClassNamespace, contextName, srcDirectory, projectBaseName, featureProperties);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetCommandFileText(string classNamespace, string contextName, string srcDirectory, string projectBaseName, NewFeatureProperties featureProperties)
        {
            var featureClassName = featureProperties.Feature;
            var commandName = featureProperties.Command;
            var returnPropType = featureProperties.ResponseType;

            var exceptionsClassPath = ClassPathHelper.CoreExceptionClassPath(srcDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var returnValue = GetReturnValue(returnPropType);

            var mtUsing = featureProperties.IsProducer ? @"
    using MassTransit;" : "";

            var handlerCtor = featureProperties.IsProducer ? $@"private readonly {contextName} _db;
            private readonly IMapper _mapper;
            private readonly IPublishEndpoint _publishEndpoint;

            public Handler({contextName} db, IMapper mapper, IPublishEndpoint publishEndpoint)
            {{
                _mapper = mapper;
                _db = db;
                _publishEndpoint = publishEndpoint;
            }}" :
            $@"private readonly {contextName} _db;
            private readonly IMapper _mapper;

            public Handler({contextName} db, IMapper mapper)
            {{
                _mapper = mapper;
                _db = db;
            }}";

            return @$"namespace {classNamespace}
{{
    using {exceptionsClassPath.ClassNamespace};
    using {contextClassPath.ClassNamespace};
    using AutoMapper;
    using AutoMapper.QueryableExtensions;{mtUsing}
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public static class {featureClassName}
    {{
        public class {commandName} : IRequest<{returnPropType}>
        {{
            public {commandName}()
            {{
            }}
        }}

        public class Custom{featureClassName}Validation
        {{
            public Custom{featureClassName}Validation()
            {{
            }}
        }}

        public class Handler : IRequestHandler<{commandName}, {returnPropType}>
        {{
            {handlerCtor}

            public async Task<{returnPropType}> Handle({commandName} request, CancellationToken cancellationToken)
            {{
                // Add your command logic for your feature here!

                return {returnValue}; // change this
            }}
        }}
    }}
}}";
        }

        //var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "char", "byte", "ushort", "uint", "ulong" };
        public static string GetReturnValue(string propType) => propType switch
        {
            "bool" => "true",
            "string" => @$"""TBD Return Value""",
            "int" => "0",
            //"float" => "new float()",
            //"double" => "new double()",
            //"decimal" => "new decimal()",
            //"DateTime" => "new DateTime()",
            //"DateOnly" => "new DateOnly()",
            //"TimeOnly" => "new TimeOnly()",
            "Guid" => "Guid.NewGuid()",
            _ => $"new {propType}()"
        };
    }
}