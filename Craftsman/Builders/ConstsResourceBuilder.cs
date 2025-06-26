namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class ConstsResourceBuilder
{
    public sealed record ConstsResourceBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<ConstsResourceBuilderCommand>
    {
        public Task Handle(ConstsResourceBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiResourcesClassPath(scaffoldingDirectoryStore.SrcDirectory, "Consts.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetConfigText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private static string GetConfigText(string classNamespace)
        {
            // lang=csharp
            return $$"""
                     namespace {{classNamespace}};

                     using System.Reflection;

                     public static class Consts
                     {
                         public static class Testing
                         {
                             public const string IntegrationTestingEnvName = "LocalIntegrationTesting";
                             public const string FunctionalTestingEnvName = "LocalFunctionalTesting";
                         }

                         public static class HangfireQueues
                         {
                             public const string Default = "default";
                             // public const string MyFirstQueue = "my-first-queue";
                             
                             public static string[] List()
                             {
                                 return typeof(HangfireQueues)
                                     .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                     .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                                     .Select(x => (string)x.GetRawConstantValue())
                                     .ToArray();
                             }
                         }
                     }
                     """;
        }
    }
}
