using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Logging
{
    /// <summary>
    /// An interface which is used for logging various events in ElasticLINQ.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message to the logging system.
        /// </summary>
        /// <param name="type">The event type of the message.</param>
        /// <param name="ex">The exception (optional).</param>
        /// <param name="additionalInfo">Additional information to be logged (optional).</param>
        /// <param name="messageFormat">The message (will be formatted, if <paramref name="args"/> is not null/empty; otherwise,
        /// should be sent directly to the logging system).</param>
        /// <param name="args">The arguments for <paramref name="messageFormat"/> (optional).</param>
        void Log(TraceEventType type, Exception ex, IDictionary<string, object> additionalInfo, string messageFormat, params object[] args);
    }
}
