using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamworkMsprojectExportTweaker
{
    [Serializable]
    public class RetryLimitException : Exception
    {
        public RetryLimitException() { }
        public RetryLimitException(string message) : base(message) { }
        public RetryLimitException(string message, Exception inner) : base(message, inner) { }
        protected RetryLimitException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
