using System;
using System.Collections.Generic;

namespace GoogleAnalytics
{
    internal sealed class Payload
    {
        public Payload(IDictionary<string, string> data)
        {
            Data = data;
            TimeStamp = DateTime.UtcNow;
        }

        public IDictionary<string, string> Data { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public bool IsUseSecure { get; set; }
    }
}
