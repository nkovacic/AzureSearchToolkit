using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Logging
{
    public sealed class NullLogger : ILogger
    {
        /// <summary>
        /// Gets the singleton <see cref="NullLogger"/> instance.
        /// </summary>
        public static readonly NullLogger Instance = new NullLogger();

        public void Log(TraceEventType type, Exception ex, IDictionary<string, object> additionalInfo, string messageFormat, params object[] args)
        {
            return;
        }
    }
}
