using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyOPDS.Properties;

namespace TinyOPDS.Data
{
    public static class LibraryFactory
    {
        private static Object thisLock = new Object();
        private static ILibrary _library = null;
        public static ILibrary GetLibrary()
        {
            lock (thisLock)
            {
                if (_library == null)
                {
                    if (Settings.Default.LibraryKind == 0)
                        _library = new Library();
                    else if (Settings.Default.LibraryKind == 1)
                        _library = new MyHomeLibrary();
                }
            }
            return _library;
        }
    }
}
