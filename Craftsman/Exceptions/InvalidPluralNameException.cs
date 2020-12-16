namespace Craftsman.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    class InvalidPluralNameException : Exception
    {
        public InvalidPluralNameException() : base($"One of your entity plural names are the same as their name.  Please make sure they are unique values.")
        {

        }
    }
}
