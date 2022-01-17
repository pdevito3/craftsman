namespace Craftsman.Builders.Dtos
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class BasePaginationParametersBuilder
    {
        public static void CreateBasePaginationParameters(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.SharedDtoClassPath(solutionDirectory, $"BasePaginationParameters.cs");

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetBasePaginationParametersText(classPath.ClassNamespace);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetBasePaginationParametersText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    public abstract class BasePaginationParameters
    {{
        internal virtual int MaxPageSize {{ get; }} = 20;
        internal virtual int DefaultPageSize {{ get; set; }} = 10;

        public virtual int PageNumber {{ get; set; }} = 1;

        public int PageSize
        {{
            get
            {{
                return DefaultPageSize;
            }}
            set
            {{
                DefaultPageSize = value > MaxPageSize ? MaxPageSize : value;
            }}
        }}
    }}
}}";
        }
    }
}