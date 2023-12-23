namespace Craftsman.Exceptions;

using System;

internal class IsNotBoundedContextDirectoryException : Exception, ICraftsmanException
{
    public IsNotBoundedContextDirectoryException() : base($"This is not a valid directory for this operation. Please make sure you are in the bounded context directory for your project (contains 'src' and 'tests' directories).")
    {
    }
}
