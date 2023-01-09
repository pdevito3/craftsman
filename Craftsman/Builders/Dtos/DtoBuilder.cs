namespace Craftsman.Builders.Dtos;

using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class DtoBuilder
{
    private readonly ICraftsmanUtilities _utilities;
    private readonly IFileSystem _fileSystem;

    public DtoBuilder(ICraftsmanUtilities utilities, IFileSystem fileSystem)
    {
        _utilities = utilities;
        _fileSystem = fileSystem;
    }

    public void CreateDtos(string srcDirectory, Entity entity, string projectBaseName, bool overwrite = false)
    {
        // ****this class path will have an invalid FullClassPath. just need the directory
        var classPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        CreateDtoFile(srcDirectory, entity, Dto.Read, projectBaseName, overwrite);
        CreateDtoFile(srcDirectory, entity, Dto.Manipulation, projectBaseName, overwrite);
        CreateDtoFile(srcDirectory, entity, Dto.Creation, projectBaseName, overwrite);
        CreateDtoFile(srcDirectory, entity, Dto.Update, projectBaseName, overwrite );
        CreateDtoFile(srcDirectory, entity, Dto.ReadParamaters, projectBaseName, overwrite);
    }

    public void CreateDtoFile(string srcDirectory, Entity entity, Dto dto, string projectBaseName, bool overwrite = false)
    {
        var dtoFileName = $"{FileNames.GetDtoName(entity.Name, dto)}.cs";
        var classPath = ClassPathHelper.DtoClassPath(srcDirectory, dtoFileName, entity.Plural, projectBaseName);
        var fileText = GetDtoFileText(srcDirectory, classPath, entity, dto);
        _utilities.CreateFile(classPath, fileText, overwrite);
    }

    public static string GetDtoFileText(string srcDirectory, ClassPath classPath, Entity entity, Dto dto)
    {
        if (dto == Dto.ReadParamaters)
            return DtoFileTextGenerator.GetReadParameterDtoText(srcDirectory, classPath.ClassNamespace, entity, dto);

        return DtoFileTextGenerator.GetDtoText(classPath, entity, dto);
    }
}
