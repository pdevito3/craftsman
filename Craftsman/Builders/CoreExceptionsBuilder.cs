namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class CoreExceptionsBuilder
    {
        public static void CreateExceptions(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            CreateNotFoundException(srcDirectory, projectBaseName, fileSystem);
            CreateValidationException(srcDirectory, projectBaseName, fileSystem);
            CreateForbiddenException(srcDirectory, projectBaseName, fileSystem);
        }

        public static void CreateValidationException(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, $"ValidationException.cs", projectBaseName);
            var fileText = GetValidationExceptionFileText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateNotFoundException(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, $"NotFoundException.cs", projectBaseName);
            var fileText = GetNotFoundExceptionFileText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateForbiddenException(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, $"ForbiddenException.cs", projectBaseName);
            var fileText = GetForbiddenExceptionFileText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetNotFoundExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

public class NotFoundException : Exception
{{
    public NotFoundException()
        : base()
    {{
    }}

    public NotFoundException(string message)
        : base(message)
    {{
    }}

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {{
    }}

    public NotFoundException(string name, object key)
        : base($""Entity \""{{name}}\"" ({{key}}) was not found."")
    {{
    }}
}}";
        }

        public static string GetForbiddenExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using System;
using System.Globalization;

public class ForbiddenAccessException : Exception
{{
    public ForbiddenAccessException() : base() {{ }}
}}";
        }

        public static string GetValidationExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using FluentValidation.Results;
using System.Collections.Generic;

public class ValidationException : Exception
{{
    public ValidationException()
        : base(""One or more validation failures have occurred."")
    {{
        Errors = new Dictionary<string, string[]>();
    }}

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {{
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }}

    public IDictionary<string, string[]> Errors {{ get; }}
}}";
        }
    }
}