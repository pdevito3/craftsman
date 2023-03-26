namespace Craftsman.Builders.EntityModels;

using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class EntityModelBuilder
{
    private readonly ICraftsmanUtilities _utilities;
    private readonly IFileSystem _fileSystem;

    public EntityModelBuilder(ICraftsmanUtilities utilities, IFileSystem fileSystem)
    {
        _utilities = utilities;
        _fileSystem = fileSystem;
    }

    public void CreateEntityModels(string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        CreateEntityModelFile(srcDirectory, entity, EntityModel.Creation, projectBaseName);
        CreateEntityModelFile(srcDirectory, entity, EntityModel.Update, projectBaseName);
    }

    public void CreateEntityModelFile(string srcDirectory, Entity entity, EntityModel model, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, model, projectBaseName);
        var fileText = GetEntityModelFileText(classPath, entity, model);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetEntityModelFileText(ClassPath classPath, Entity entity, EntityModel model)
    {
        return EntityModelFileTextGenerator.GetEntityModelText(classPath, entity, model);
    }
}
