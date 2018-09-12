using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Logging
{
    public enum TraceEventType
    {
        /// <summary>
        /// Fatal error or application crash.
        /// </summary>
        Critical = 1,

        /// <summary>
        /// Recoverable error.
        /// </summary>
        Error = 2,

        /// <summary>
        /// Noncritical problem.
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Informational message.
        /// </summary>
        Information = 8,

        /// <summary>
        /// Debugging trace.
        /// </summary>
        Verbose = 16,
    }
}
