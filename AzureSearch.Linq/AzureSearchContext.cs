using AzureSearch.Linq.Logging;
using AzureSearch.Linq.Utlities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearch.Linq
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

        public AzureSearchContext(IAzureSearchConnection connection, ILogger logger = null)
        {
            Argument.EnsureNotNull(nameof(connection), connection);

            Connection = connection;
            Logger = logger ?? NullLogger.Instance;
        }

        public IQueryable<T> Query<T>()
        {
            throw new AzureSearchQuery<T>(new AzureSearchQueryProvider());
        }
    }
}
