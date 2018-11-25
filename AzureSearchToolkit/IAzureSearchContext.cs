using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchToolkit
{
    /// <summary>
    /// Represents a unit of work in AzureSearchToolkit.
    /// </summary>
    public interface IAzureSearchContext
    {
        /// <summary>
        /// Add document to index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">Document to add to index</param>
        /// <returns>If document is successfully added, true is returned, otherwise false </returns>
        Task<bool> AddAsync<T>(T document) where T: class;

        /// <summary>
        /// Add documents to index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="documents">Documents to add to index</param>
        /// <returns>If documents is successfully added, true is returned, otherwise false </returns>
        Task<bool> AddAsync<T>(IEnumerable<T> documents) where T : class;

        Task<bool> AddOrUpdateAsync<T>(T document) where T : class;
        Task<bool> AddOrUpdateAsync<T>(IEnumerable<T> documents) where T : class;

        Task<bool> RemoveAsync<T>(T document) where T : class;
        Task<bool> RemoveAsync<T>(IEnumerable<T> documents) where T : class;

        Task<bool> UpdateAsync<T>(T document) where T : class;
        Task<bool> UpdateAsync<T>(IEnumerable<T> documents) where T : class;

        /// <summary>
        /// Gets a query that can search for documents of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The document type.</typeparam>
        /// <returns>The query that can search for documents of the given type.</returns>
        IQueryable<T> Query<T>() where T : class;
    }
}
