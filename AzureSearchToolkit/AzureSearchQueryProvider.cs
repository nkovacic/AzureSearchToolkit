using Azure.Core.Serialization;
using Azure.Search.Documents;
using AzureSearchToolkit.Request.Formatters;
using AzureSearchToolkit.Request.Visitors;
using AzureSearchToolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AzureSearchToolkit
{
    public sealed class AzureSearchQueryProvider<T> : IQueryProvider
    {
        internal SearchClient SearchClient { get; private set; }
        private readonly JsonSerializerOptions jsonOptions;

        /// <summary>
        /// Create a new AzureSearchQueryProvider for a given connection, logger and field prefix.
        /// </summary>
        /// <param name="searchClient">Connection to use to connect to Elasticsearch.</param>
        public AzureSearchQueryProvider(SearchClient searchClient, JsonSerializerOptions jsonOptions)
        {
            Argument.EnsureNotNull(nameof(searchClient), searchClient);

            SearchClient = searchClient;
            this.jsonOptions = jsonOptions;
        }

        /// <inheritdoc />
        IQueryable<TResult> IQueryProvider.CreateQuery<TResult>(Expression expression)
        {
            if (!typeof(TResult).IsAssignableFrom(typeof(T)))
                throw new NotSupportedException();

            Argument.EnsureNotNull(nameof(expression), expression);

            if (!typeof(IQueryable<TResult>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }

            return new AzureSearchQuery<T, TResult>(this, expression);
        }

        /// <inheritdoc />
        public IQueryable CreateQuery(Expression expression)
        {
            Argument.EnsureNotNull(nameof(expression), expression);

            var elementType = TypeHelper.GetSequenceElementType(expression.Type);
            var queryType = typeof(AzureSearchQuery<,>).MakeGenericType(typeof(T), elementType);

            try
            {
                return (IQueryable)Activator.CreateInstance(queryType, this, expression);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();

                return null;  // Never called, as the above code re-throws
            }
        }

        /// <inheritdoc/>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        /// <inheritdoc/>
        public object Execute(Expression expression)
        {
            Argument.EnsureNotNull(nameof(expression), expression);
            var translation = AzureSearchQueryTranslator<T>.Translate(expression, jsonOptions);
            var formatter = new SearchRequestFormatter(translation.SearchRequest);
            var searchRequest = formatter.SearchRequest;
            var response = SearchClient.Search<T>(searchRequest.SearchText, searchRequest.SearchOptions);
            return translation.Materializer.Materialize(response);
        }
    }
}
