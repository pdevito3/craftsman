namespace Craftsman.Exceptions;

using System;

[Serializable]
public class InvalidTemplateException : Exception, ICraftsmanException
{
    public InvalidTemplateException(string message) : base(message)
    {
    }

    public InvalidTemplateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
