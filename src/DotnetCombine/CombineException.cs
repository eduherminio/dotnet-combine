using System;
using System.Runtime.Serialization;

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

        protected CombineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
