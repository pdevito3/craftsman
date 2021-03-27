namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class PagedListBuilder
    {
        public static void CreatePagedList(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.WrappersClassPath(solutionDirectory, $"PagedList.cs", projectBaseName);

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetPagedListText(classPath.ClassNamespace);
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

        public static string GetPagedListText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class PagedList<T> : List<T>
    {{
        public int PageNumber {{ get; private set; }}
        public int TotalPages {{ get; private set; }}
        public int PageSize {{ get; private set; }}
        public int CurrentPageSize {{ get; set; }}
        public int CurrentStartIndex {{ get; set; }}
        public int CurrentEndIndex {{ get; set; }}
        public int TotalCount {{ get; private set; }}

        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {{
            TotalCount = count;
            PageSize = pageSize;
            PageNumber = pageNumber;
            CurrentPageSize = items.Count;
            CurrentStartIndex = count == 0 ? 0 : ((pageNumber - 1) * pageSize) + 1;
            CurrentEndIndex = count == 0 ? 0 : CurrentStartIndex + CurrentPageSize - 1;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }}

        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {{
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }}

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {{
            var count = source.Count();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }}
    }}
}}";
        }
    }
}