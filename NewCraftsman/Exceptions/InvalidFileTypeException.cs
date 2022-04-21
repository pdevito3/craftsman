namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    class InvalidFileTypeException : Exception, ICraftsmanException
    {
        public InvalidFileTypeException() : base($"Invalid file type. You need to use a json or yml file.")
        {

        }
    }
}
