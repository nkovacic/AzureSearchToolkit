using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using AzureSearchToolkit.Request.Formatters;
using AzureSearchToolkit.Request.Visitors;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AzureSearchToolkit
{
    public static class SearchClientExtensions
    {
        /// <summary>
        /// Gets a query that can search for documents of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The document type.</typeparam>
        /// <returns>The query that can search for documents of the given type.</returns>
        public static IQueryable<T> Query<T>(this SearchClient searchClient, JsonSerializerOptions jsonSerializerOptions = null)
        {
            return new AzureSearchQuery<T, T>(new AzureSearchQueryProvider<T>(searchClient, jsonSerializerOptions ?? new JsonSerializerOptions()));
        }

        public static Task<Response<SearchResults<T>>> SearchAsync<T>(this SearchClient searchClient, IQueryable<T> query, JsonSerializerOptions jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            return searchClient.SearchAsync<T>(query.Expression, jsonSerializerOptions, cancellationToken);
        }

        public static Task<Response<SearchResults<T>>> SearchAsync<T>(this SearchClient searchClient, Expression expression, JsonSerializerOptions jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            var translation = AzureSearchQueryTranslator<T>.Translate(expression, jsonSerializerOptions ?? new JsonSerializerOptions());
            var formatter = new SearchRequestFormatter(translation.SearchRequest);
            var searchRequest = formatter.SearchRequest;
            return searchClient.SearchAsync<T>(searchRequest.SearchText, searchRequest.SearchOptions, cancellationToken);
        }
    }
}
