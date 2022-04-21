namespace NewCraftsman.Builders
{
    using System.IO;
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class CoreExceptionsBuilder
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IFileSystem _fileSystem;

        public CoreExceptionsBuilder(ICraftsmanUtilities utilities, IFileSystem fileSystem)
        {
            _utilities = utilities;
            _fileSystem = fileSystem;
        }

        public void CreateExceptions(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            CreateNotFoundException(solutionDirectory, projectBaseName);
            CreateValidationException(solutionDirectory, projectBaseName);
            CreateForbiddenException(solutionDirectory, projectBaseName);
        }

        public void CreateValidationException(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"ValidationException.cs");
            var fileText = GetValidationExceptionFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }

        public void CreateNotFoundException(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"NotFoundException.cs");
            var fileText = GetNotFoundExceptionFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }

        public void CreateForbiddenException(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"ForbiddenException.cs");
            var fileText = GetForbiddenExceptionFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetNotFoundExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System;

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
    }}
}}";
        }

        public static string GetForbiddenExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System;
    using System.Globalization;

    public class ForbiddenAccessException : Exception
    {{
        public ForbiddenAccessException() : base() {{ }}
    }}
}}";
        }

        public static string GetValidationExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using FluentValidation.Results;
    using System.Collections.Generic;
    using System.Linq;
    using System;

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
    }}
}}";
        }
    }
}