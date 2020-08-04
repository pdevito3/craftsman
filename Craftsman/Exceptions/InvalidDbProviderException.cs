using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Exceptions
{
    public class InvalidDbProviderException : Exception
    {
        public InvalidDbProviderException() : base($"The given database provider was not recognized.")
        {

        }

        public InvalidDbProviderException(string dbProvider) : base($"The database provider `{dbProvider}` was not recognized.")
        {

        }
    }
}
