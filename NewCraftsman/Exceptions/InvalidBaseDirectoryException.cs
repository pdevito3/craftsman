namespace NewCraftsman.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class IsNotBoundedContextDirectory : Exception
    {
        public IsNotBoundedContextDirectory() : base($"This is not a valid directory for this operation. Please make sure you are in the bounded context directory for your project (contains 'src' and 'tests' directories).")
        {
        }

        public IsNotBoundedContextDirectory(string message) : base(message)
        {
        }

        public IsNotBoundedContextDirectory(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IsNotBoundedContextDirectory(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}