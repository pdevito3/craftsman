namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    class DirectoryAlreadyExistsException : Exception, ICraftsmanException
    {
        public DirectoryAlreadyExistsException() : base($"This directory already exists.")
        {

        }

        public DirectoryAlreadyExistsException(string directory) : base($"The directory `{directory}` already exists.")
        {

        }
    }
}
