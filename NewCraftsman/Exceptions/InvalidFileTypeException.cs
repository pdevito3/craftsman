namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    class InvalidFileTypeException : Exception
    {
        public InvalidFileTypeException() : base($"Invalid file type. You need to use a json or yml file.")
        {

        }
    }
}
