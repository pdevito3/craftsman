namespace NewCraftsman.Builders.Bff.Src;

using System.IO.Abstractions;
using Helpers;
using Services;

public class TypesBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public TypesBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiTypes(string spaDirectory)
    {
        var classPath = ClassPathHelper.BffSpaSrcApiTypesClassPath(spaDirectory, "index.ts");
        var fileText = GetApiTypesText();
        _utilities.CreateFile(classPath, fileText);
    }
    
    public static string GetApiTypesText()
    {
            return @$"export interface PagedResponse<T> {{
  pagination: Pagination;
  data: T[];
}}

export interface Pagination {{
  currentEndIndex: number;
  currentPageSize: number;
  currentStartIndex: number;
  hasNext: boolean;
  hasPrevious: boolean;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}}";
    }
}