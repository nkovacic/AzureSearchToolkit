using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit
{
    public class AzureSearchContext : IAzureSearchContext
    {
        /// <summary>
        /// Specifies the connection to the AzureSearch instance.
        /// </summary>
        public IAzureSearchConnection Connection { get; }

        /// <summary>
        /// The logging mechanism for diagnostic information.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// The mapping to describe how objects and their properties are mapped to AzureSearch.
        /// </summary>
        public IAzureSearchMapping Mapping { get; }

        public AzureSearchContext(IAzureSearchConnection connection, IAzureSearchMapping mapping = null, ILogger logger = null)
        {
            Argument.EnsureNotNull(nameof(connection), connection);

            Connection = connection;
            Mapping = mapping ?? new AzureSearchMapping();
            Logger = logger ?? NullLogger.Instance;
        }

        public IQueryable<T> Query<T>()
        {
            return new AzureSearchQuery<T>(new AzureSearchQueryProvider(Connection, Mapping, Logger));
        }
    }
}
