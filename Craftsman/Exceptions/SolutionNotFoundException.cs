namespace Craftsman.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class SolutionNotFoundException : Exception, ICraftsmanException
    {
        public SolutionNotFoundException() : base($"A solution file was not found in your current durectory. Please make sure you are in the solution directory for your project.")
        {
        }

        public SolutionNotFoundException(string message) : base(message)
        {
        }

        public SolutionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SolutionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}