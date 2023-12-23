namespace Craftsman.Exceptions;

using System;

internal class SolutionNameEntityMatchException : Exception, ICraftsmanException
{
    public SolutionNameEntityMatchException() : base($"Your solution name can not match an entity name. This will cause namespace issues in your project.")
    {
    }
}
