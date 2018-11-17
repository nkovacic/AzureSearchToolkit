using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<bool> AddAsync<T>(T document) where T: class
        {
            return await ChangeDocumentAsync(document, IndexActionType.Upload);
        }

        public async Task<bool> AddAsync<T>(IEnumerable<T> documents) where T : class
        {
            return await ChangeDocumentAsync(documents, IndexActionType.Upload);
        }

        public async Task<bool> AddOrUpdateAsync<T>(T document) where T : class
        {
            return await ChangeDocumentAsync(document, IndexActionType.MergeOrUpload);
        }

        public async Task<bool> AddOrUpdateAsync<T>(IEnumerable<T> documents) where T : class
        {
            return await ChangeDocumentAsync(documents, IndexActionType.MergeOrUpload);
        }

        public async Task<bool> RemoveAsync<T>(T document) where T : class
        {
            return await ChangeDocumentAsync(document, IndexActionType.Delete);
        }

        public async Task<bool> RemoveAsync<T>(IEnumerable<T> documents) where T : class
        {
            return await ChangeDocumentAsync(documents, IndexActionType.Delete);
        }

        public async Task<bool> UpdateAsync<T>(T document) where T : class
        {
            return await ChangeDocumentAsync(document, IndexActionType.Upload);
        }

        public async Task<bool> UpdateAsync<T>(IEnumerable<T> documents) where T : class
        {
            return await ChangeDocumentAsync(documents, IndexActionType.Upload);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return new AzureSearchQuery<T>(new AzureSearchQueryProvider(Connection, Mapping, Logger));
        }

        private async Task<bool> ChangeDocumentAsync<T>(T document, IndexActionType indexActionType) where T : class
        {
            Argument.EnsureNotNull(nameof(document), document);

            return await Connection.ChangeDocumentsInIndexAsync(new Dictionary<T, IndexActionType>()
            {
                { document, indexActionType }
            });
        }

        private async Task<bool> ChangeDocumentAsync<T>(IEnumerable<T> documents, IndexActionType indexActionType) where T : class
        {
            Argument.EnsureNotEmpty(nameof(documents), documents);

            return await Connection.ChangeDocumentsInIndexAsync(documents.ToDictionary(
                q => q,
                q => indexActionType
            ));
        }
    }
}
