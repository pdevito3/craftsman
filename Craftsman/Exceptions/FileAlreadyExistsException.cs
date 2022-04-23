namespace Craftsman.Exceptions
{
    using System;

    [Serializable]
    public class FileAlreadyExistsException : Exception, ICraftsmanException
    {
        public FileAlreadyExistsException() : base($"This file already exists.")
        {

        }

        public FileAlreadyExistsException(string file) : base($"The file `{file}` already exists.")
        {

        }
    }
}
