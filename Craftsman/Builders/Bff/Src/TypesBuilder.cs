namespace Craftsman.Builders.Bff.Src;

using System;
using System.IO.Abstractions;
using System.Linq;
using Enums;
using Helpers;
using Models;
using static Helpers.ConstMessages;

public class TypesBuilder
{
    public static void CreateApiTypes(string spaDirectory, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.BffSpaSrcApiTypesClassPath(spaDirectory, "index.ts");
        var fileText = GetApiTypesText();
        Utilities.CreateFile(classPath, fileText, fileSystem);
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