using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit
{
    /// <summary>
    /// Represents a unit of work in AzureSearchLINQ.
    /// </summary>
    public interface IAzureSearchContext
    {
        /// <summary>
        /// Gets a query that can search for documents of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The document type.</typeparam>
        /// <returns>The query that can search for documents of the given type.</returns>
        IQueryable<T> Query<T>();
    }
}
