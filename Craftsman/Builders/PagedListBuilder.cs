namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class PagedListBuilder
{
    public sealed record PagedListBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<PagedListBuilderCommand>
    {
        public Task Handle(PagedListBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiResourcesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"PagedList.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetPagedListText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
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
