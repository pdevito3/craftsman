namespace Craftsman.Builders.Tests.IntegrationTests;

using System;
using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Humanizer;
using MediatR;

public static class JobFeatureIntegrationTestBuilder
{
    public record Command(Feature Feature, string EntityPlural) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(_scaffoldingDirectoryStore.TestDirectory, 
                $"{request.Feature.Name}Tests.cs", 
                request.EntityPlural, 
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath, request.Feature, request.EntityPlural);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(ClassPath classPath, Feature feature, string entityPlural)
        {
            var featuresClassPath = ClassPathHelper.FeaturesClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"",
                entityPlural,
                _scaffoldingDirectoryStore.ProjectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                $"",
                _scaffoldingDirectoryStore.ProjectBaseName);
            
            return @$"namespace {classPath.ClassNamespace};

using {featuresClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using Bogus;
using Domain;
using System.Threading.Tasks;

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    [Fact]
    public async Task can_perform_{feature.Name.Humanize().Underscore()}()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var user = Guid.NewGuid().ToString();
        var uow = testingServiceScope.GetService<IUnitOfWork>();

        // Act
        var job = new {feature.Name}(uow);
        var command = new {feature.Name}.Command() {{ User = user }};
        await job.Handle(command, CancellationToken.None);

        // Assert
        // TODO job assertion
    }}
}}";
        }
    }
}