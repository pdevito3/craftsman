namespace Craftsman.Builders.Dtos;

using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class DtoBuilder
{
    public sealed record CreateDtosCommand(Entity Entity) : IRequest;

    public class CreateDtosHandler(ICraftsmanUtilities utilities, IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CreateDtosCommand>
    {
        public Task Handle(CreateDtosCommand request, CancellationToken cancellationToken)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.DtoClassPath(scaffoldingDirectoryStore.SrcDirectory, "", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            CreateDtoFile(scaffoldingDirectoryStore.SrcDirectory, request.Entity, Dto.Read, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            CreateDtoFile(scaffoldingDirectoryStore.SrcDirectory, request.Entity, Dto.Creation, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            CreateDtoFile(scaffoldingDirectoryStore.SrcDirectory, request.Entity, Dto.Update, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            CreateDtoFile(scaffoldingDirectoryStore.SrcDirectory, request.Entity, Dto.ReadParamaters, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            return Task.CompletedTask;
        }
    }

    public sealed record CreateDtoFileCommand(Entity Entity, Dto Dto) : IRequest;

    public class CreateDtoFileHandler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CreateDtoFileCommand>
    {
        public Task Handle(CreateDtoFileCommand request, CancellationToken cancellationToken)
        {
            CreateDtoFile(scaffoldingDirectoryStore.SrcDirectory, request.Entity, request.Dto, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            return Task.CompletedTask;
        }
    }

    private static void CreateDtoFile(string srcDirectory, Entity entity, Dto dto, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var dtoFileName = $"{FileNames.GetDtoName(entity.Name, dto)}.cs";
        var classPath = ClassPathHelper.DtoClassPath(srcDirectory, dtoFileName, entity.Plural, projectBaseName);
        var fileText = GetDtoFileText(srcDirectory, classPath, entity, dto, projectBaseName);
        utilities.CreateFile(classPath, fileText);
    }

    public static string GetDtoFileText(string srcDirectory, ClassPath classPath, Entity entity, Dto dto, string projectBaseName)
    {
        if (dto == Dto.ReadParamaters)
            return DtoFileTextGenerator.GetReadParameterDtoText(srcDirectory, classPath.ClassNamespace, entity, dto, projectBaseName);

        return DtoFileTextGenerator.GetDtoText(classPath, entity, dto);
    }
}