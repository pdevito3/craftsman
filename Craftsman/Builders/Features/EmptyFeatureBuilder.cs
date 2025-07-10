namespace Craftsman.Builders.Features;

using Domain;
using Helpers;
using Services;
using MediatR;

public static class EmptyFeatureBuilder
{
    public sealed record Command(string ContextName, Feature NewFeature) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.NewFeature.Name}.cs", request.NewFeature.EntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, request.ContextName, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.NewFeature);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetCommandFileText(string classNamespace, string contextName, string srcDirectory,
        string projectBaseName, Feature newFeature)
    {
        var featureClassName = newFeature.Name;
        var returnPropType = newFeature.ResponseType;

        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var returnValue = GetReturnValue(returnPropType);

        var handlerCtor = $@"private readonly {contextName} _db;

            public Handler({contextName} db)
            {{
                _db = db;
            }}";

        return @$"namespace {classNamespace};

using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class {featureClassName}
{{
    public sealed record Request() : IRequest<{returnPropType}>;

    public sealed class Handler : IRequestHandler<Request, {returnPropType}>
    {{
        {handlerCtor}

        public async Task<{returnPropType}> Handle(Request request, CancellationToken cancellationToken)
        {{
            // Add your logic for your feature here!

            return {returnValue};
        }}
    }}
}}";
    }

    //var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "char", "byte", "ushort", "uint", "ulong" };
    private static string GetReturnValue(string propType) => propType switch
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