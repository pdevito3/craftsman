namespace NewCraftsman.Builders.Dtos
{
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using Domain;
    using Domain.Enums;
    using Exceptions;
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

        public void CreateDtos(string solutionDirectory, Entity entity, string projectBaseName)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            CreateDtoFile(solutionDirectory, entity, Dto.Read, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Manipulation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Creation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Update, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.ReadParamaters, projectBaseName);
        }

        public void CreateDtoFile(string solutionDirectory, Entity entity, Dto dto, string projectBaseName)
        {
            var dtoFileName = $"{FileNames.GetDtoName(entity.Name, dto)}.cs";
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entity.Name, projectBaseName);
            var fileText = GetDtoFileText(solutionDirectory, classPath, entity, dto, projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetDtoFileText(string solutionDirectory, ClassPath classPath, Entity entity, Dto dto, string projectBaseName)
        {
            if (dto == Dto.ReadParamaters)
                return DtoFileTextGenerator.GetReadParameterDtoText(solutionDirectory, classPath.ClassNamespace, entity, dto);
            
            return DtoFileTextGenerator.GetDtoText(classPath, entity, dto);
        }
    }
}