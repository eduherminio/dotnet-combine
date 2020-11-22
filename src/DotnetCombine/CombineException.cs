using System;

namespace DotnetCombine
{
    [Serializable]
    public class CombineException : Exception
    {
        public CombineException() : base()
        {
        }

        public CombineException(string? message) : base(message)
        {
        }

        public CombineException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
