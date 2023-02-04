namespace Craftsman.Builders.Tests.Utilities;

using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class UnitTestUtilsBuilder
{
    public class Command : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.UnitTestHelpersClassPath(_scaffoldingDirectoryStore.TestDirectory, $"{FileNames.UnitTestUtilsName()}.cs", _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using System.Reflection;
using Mapster;
using MapsterMapper;
using Services;

public class UnitTestUtils
{{
    public static Mapper GetApiMapper()
    {{
        var apiAssembly = GetApiAssembly();
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings.Clone();
        typeAdapterConfig.Scan(apiAssembly);
        var mapper = new Mapper(typeAdapterConfig);
        return mapper;
    }}

    public static Assembly GetApiAssembly()
    {{
        // need to load something from the api for it to be in the loaded assemblies
        _ = new DateTimeProvider();
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == ""{_scaffoldingDirectoryStore.ProjectBaseName}"");
    }}
}}
";
        }
    }
}
