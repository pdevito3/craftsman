namespace Craftsman.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class InvalidBaseDirectory : Exception
    {
        public InvalidBaseDirectory() : base($"This is not a valid directory for this operation. Please make sure you are in the solution directory for your project.")
        {
        }

        public InvalidBaseDirectory(string message) : base(message)
        {
        }

        public InvalidBaseDirectory(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidBaseDirectory(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}