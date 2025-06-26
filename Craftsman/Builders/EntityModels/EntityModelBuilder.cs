namespace Craftsman.Builders.EntityModels;

using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class EntityModelBuilder
{
    public sealed record EntityModelBuilderCommand(Entity Entity) : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<EntityModelBuilderCommand>
    {
        public Task Handle(EntityModelBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.EntityModelClassPath(scaffoldingDirectoryStore.SrcDirectory, request.Entity.Name, request.Entity.Plural, null, scaffoldingDirectoryStore.ProjectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            CreateEntityModelFile(scaffoldingDirectoryStore.SrcDirectory, request.Entity, EntityModel.Creation, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            CreateEntityModelFile(scaffoldingDirectoryStore.SrcDirectory, request.Entity, EntityModel.Update, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            return Task.CompletedTask;
        }
    }

    private static void CreateEntityModelFile(string srcDirectory, Entity entity, EntityModel model, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var classPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, model, projectBaseName);
        var fileText = GetEntityModelFileText(classPath, entity, model);
        utilities.CreateFile(classPath, fileText);
    }

    public static string GetEntityModelFileText(ClassPath classPath, Entity entity, EntityModel model)
    {
        return EntityModelFileTextGenerator.GetEntityModelText(classPath, entity, model);
    }
}
