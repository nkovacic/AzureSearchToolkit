using Azure;
using Azure.Search.Documents.Models;
using AzureSearchToolkit.Utilities;

namespace AzureSearchToolkit.Response.Materializers
{
    abstract class ChainMaterializer<T> : IAzureSearchMaterializer<T>
    {
        protected ChainMaterializer(IAzureSearchMaterializer<T> next)
        {
            Next = next;
        }

        public IAzureSearchMaterializer<T> Next
        {
            get; set;
        }

        /// <summary>
        /// Process response, then translate it to next materializer.
        /// </summary>
        /// <param name="response">AzureOperationResponse to obtain the existence of a result.</param>
        /// <returns>Return result of previous materializer, previously processed by self</returns>
        public object Materialize(Response<SearchResults<T>> response)
        {
            Argument.EnsureNotNull("Next materializer must be setted.", Next);

            return Next.Materialize(response);
        }
    }
}
