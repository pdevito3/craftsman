namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    public class DuplicateSingletonException : Exception, ICraftsmanException
    {
        public DuplicateSingletonException() : base($"This singleton has been instantiated more than once.")
        {

        }    
    }
}
