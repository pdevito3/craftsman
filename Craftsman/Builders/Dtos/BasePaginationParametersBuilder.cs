namespace Craftsman.Builders.Dtos
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class BasePaginationParametersBuilder
    {
        public static void CreateBasePaginationParameters(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.SharedDtoClassPath(solutionDirectory, $"BasePaginationParameters.cs", projectBaseName);

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
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
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