namespace Craftsman.Builders;

using System.Text.Json;
using Domain;
using Helpers;
using MediatR;
using Services;

public static class ExampleTemplateBuilder
{
    public sealed record CreateFileCommand(string SolutionDirectory, DomainProject DomainProject) : IRequest;
    public sealed record CreateYamlFileCommand(string SolutionDirectory, string DomainProject) : IRequest;

    public class CreateFileHandler(ICraftsmanUtilities utilities) : IRequestHandler<CreateFileCommand>
    {
        public Task Handle(CreateFileCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.ExampleYamlRootClassPath(request.SolutionDirectory, "exampleTemplate.json");
            var fileText = JsonSerializer.Serialize(request.DomainProject);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public class CreateYamlFileHandler(ICraftsmanUtilities utilities) : IRequestHandler<CreateYamlFileCommand>
    {
        public Task Handle(CreateYamlFileCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.ExampleYamlRootClassPath(request.SolutionDirectory, "exampleTemplate.yaml");
            utilities.CreateFile(classPath, request.DomainProject);
            return Task.CompletedTask;
        }
    }
}
