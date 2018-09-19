using AzureSearchToolkit.Async;
using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Mapping;
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
using System.Threading;
using System.Threading.Tasks;

namespace AzureSearchToolkit
{
    public sealed class AzureSearchQueryProvider : IQueryProvider, IAsyncQueryExecutor
    {
        internal IAzureSearchConnection Connection { get; private set; }

        internal ILogger Logger { get; }

        internal IAzureSearchMapping Mapping { get;}

        /// <summary>
        /// Create a new AzureSearchQueryProvider for a given connection, logger and field prefix.
        /// </summary>
        /// <param name="connection">Connection to use to connect to Elasticsearch.</param>
        /// <param name="mapping">A mapping to specify how queries and results are translated.</param>
        /// <param name="log">A log to receive any information or debugging messages.</param>
        /// <param name="retryPolicy">A policy to describe how to handle network issues.</param>
        public AzureSearchQueryProvider(IAzureSearchConnection connection, IAzureSearchMapping mapping, ILogger logger)
        {
            Argument.EnsureNotNull(nameof(connection), connection);
            Argument.EnsureNotNull(nameof(mapping), mapping);
            Argument.EnsureNotNull(nameof(logger), logger);

            Connection = connection;
            Mapping = mapping;
            Logger = logger;
        }

        /// <inheritdoc />
        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            Argument.EnsureNotNull(nameof(expression), expression);

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }               

            return new AzureSearchQuery<T>(this, expression);
        }

        /// <inheritdoc />
        public IQueryable CreateQuery(Expression expression)
        {
            Argument.EnsureNotNull(nameof(expression), expression);

            var elementType = TypeHelper.GetSequenceElementType(expression.Type);
            var queryType = typeof(AzureSearchQuery<>).MakeGenericType(elementType);

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
            return AsyncHelper.RunSync(() => ExecuteAsync<object>(expression));
        }

        /// <inheritdoc/>
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default(CancellationToken)) where TResult: class
        {
            return (TResult)await ExecuteAsync(expression, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            Argument.EnsureNotNull(nameof(expression), expression);

            var translation = AzureSearchQueryTranslator.Translate(Mapping, expression);
            
            //Logger.Log(TraceEventType.Information, null, null, $"Executing query against document '{translation.SearchParameters.QueryType}'");

            try
            {
                var formatter = new SearchRequestFormatter(Mapping, translation.SearchRequest);

                var response = await Connection.SearchAsync(formatter.SearchRequest.SearchParameters, 
                    translation.SearchRequest.SearchText, Logger);

                return translation.Materializer.Materialize(response);
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                return null;
            }
        }
    }
}
