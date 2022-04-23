namespace Craftsman.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class IsNotBoundedContextDirectoryException : Exception, ICraftsmanException
    {
        public IsNotBoundedContextDirectoryException() : base($"This is not a valid directory for this operation. Please make sure you are in the bounded context directory for your project (contains 'src' and 'tests' directories).")
        {
        }

        public IsNotBoundedContextDirectoryException(string message) : base(message)
        {
        }

        public IsNotBoundedContextDirectoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IsNotBoundedContextDirectoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}