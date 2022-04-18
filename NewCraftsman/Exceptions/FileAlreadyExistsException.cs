namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    public class FileAlreadyExistsException : Exception
    {
        public FileAlreadyExistsException() : base($"This file already exists.")
        {

        }

        public FileAlreadyExistsException(string file) : base($"The file `{file}` already exists.")
        {

        }
    }
}
