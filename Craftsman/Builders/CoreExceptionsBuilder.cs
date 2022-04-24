namespace Craftsman.Builders;

using System.IO.Abstractions;
using Helpers;
using MediatR;
using Services;

public static class CoreExceptionBuilder
{
    public class CoreExceptionBuilderCommand : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<CoreExceptionBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IFileSystem _fileSystem;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IFileSystem fileSystem,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _fileSystem = fileSystem;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public async Task<bool> Handle(CoreExceptionBuilderCommand request, CancellationToken cancellationToken)
        {
            CreateExceptions(_scaffoldingDirectoryStore.SolutionDirectory);

            return true;
        }

        public void CreateExceptions(string solutionDirectory)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            CreateNotFoundException(solutionDirectory);
            CreateValidationException(solutionDirectory);
            CreateForbiddenException(solutionDirectory);
        }

        public void CreateValidationException(string solutionDirectory)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"ValidationException.cs");
            var fileText = GetValidationExceptionFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }

        public void CreateNotFoundException(string solutionDirectory)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"NotFoundException.cs");
            var fileText = GetNotFoundExceptionFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }

        public void CreateForbiddenException(string solutionDirectory)
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