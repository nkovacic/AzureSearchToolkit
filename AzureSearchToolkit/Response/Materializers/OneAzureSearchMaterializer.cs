using Azure;
using Azure.Search.Documents.Models;
using AzureSearchToolkit.Utilities;
using System;

namespace AzureSearchToolkit.Response.Materializers
{
    /// <summary>
    /// Materializes one hit into a CLR object throwing necessary exceptions as required to ensure First/Single semantics.
    /// </summary>
    class OneAzureSearchMaterializer<T> : IAzureSearchMaterializer<T>
    {
        readonly bool throwIfMoreThanOne;
        readonly bool defaultIfNone;

        /// <summary>
        /// Create an instance of the OneAzureSearchMaterializer with the given parameters.
        /// </summary>
        /// <param name="throwIfMoreThanOne">Whether to throw an InvalidOperationException if there are multiple hits.</param>
        /// <param name="defaultIfNone">Whether to throw an InvalidOperationException if there are no hits.</param>
        public OneAzureSearchMaterializer(bool throwIfMoreThanOne, bool defaultIfNone)
        {
            this.throwIfMoreThanOne = throwIfMoreThanOne;
            this.defaultIfNone = defaultIfNone;
        }

        public object Materialize(Response<SearchResults<T>> response)
        {
            Argument.EnsureNotNull(nameof(response), response);

            using (var enumerator = response.Value.GetResults().GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    if (defaultIfNone)
                    {
                        return TypeHelper.CreateDefault(typeof(T));
                    }
                    else
                    {
                        throw new InvalidOperationException("Sequence contains no elements");
                    }
                }

                var current = enumerator.Current.Document;

                if (throwIfMoreThanOne && enumerator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains more than one element");
                }

                return current;
            }
        }
    }
}
