using System;

namespace Util
{
    public class UndeclaredEventException : Exception
    {
        public UndeclaredEventException() : base() { }
        public UndeclaredEventException(string message) : base(message) { }
        public UndeclaredEventException(string message, Exception inner) : base(message, inner) { }
    }
}
