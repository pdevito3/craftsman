using Craftsman.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Helpers
{
    public class GlobalSingleton
    {
        private static readonly List<string> createdFiles = new List<string>();
        private static readonly List<string> updatedFiles = new List<string>();
        private static int creationCount = 0;

        private static readonly GlobalSingleton _mySingletonServiceInstance = new GlobalSingleton();

        private GlobalSingleton()
        {
            creationCount++;
            if(creationCount > 1)
            {
                throw new DuplicateSingletonException();
            }
        }

        public static void AddCreatedFile(string val)
        {
            createdFiles.Add(val);
        }

        public static void AddUpdatedFile(string val)
        {
            updatedFiles.Add(val);
        }

        public static GlobalSingleton GetInstance() => _mySingletonServiceInstance;

        public static List<string> GetCreatedFiles() => createdFiles;

        public static List<string> GetUpdatedFiles() => updatedFiles;
    }
}
