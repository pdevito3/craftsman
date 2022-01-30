namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class PagedListBuilder
    {
        public static void CreatePagedList(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WrappersClassPath(srcDirectory, $"PagedList.cs", projectBaseName);
            var fileText = GetPagedListText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetPagedListText(string classNamespace)
        {
            return @$"namespace {classNamespace};

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

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {{
        var count = source.Count();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }}
}}";
        }
    }
}