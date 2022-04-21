namespace NewCraftsman.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class SolutiuonNameEntityMatchException : Exception, ICraftsmanException
    {
        public SolutiuonNameEntityMatchException() : base($"Your solution name can not match an entity name. This will cause namespace issues in your project.")
        {
        }

        public SolutiuonNameEntityMatchException(string message) : base(message)
        {
        }

        public SolutiuonNameEntityMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SolutiuonNameEntityMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}