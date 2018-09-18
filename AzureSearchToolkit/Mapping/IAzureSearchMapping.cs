using AzureSearchToolkit.Request.Criteria;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AzureSearchToolkit.Mapping
{
    /// <summary>
    /// Interface to describe how types and properties are mapped into AzureSearch.
    /// </summary>
    public interface IAzureSearchMapping
    {
        /// <summary>
        /// Used to format values used in AzureSearch criteria. Extending this allows you to
        /// specify rules like lower-casing values for certain types of criteria so that searched
        /// values match the rules AzureSearch is using to store/search values.
        /// </summary>
        /// <param name="member">The member that this value is searching.</param>
        /// <param name="value">The value to be formatted.</param>
        /// <returns>Returns the formatted value.</returns>
        JToken FormatValue(MemberInfo member, object value);

        /// <summary>
        /// Gets the field name for the given member. Extending this allows you to change the
        /// mapping field names in the CLR to field names in AzureSearch. Typically, these rules
        /// will need to match the serialization rules you use when storing your documents.
        /// </summary>
        /// <param name="type">The CLR type used in the source query.</param>
        /// <param name="memberExpression">The member expression whose name is required.</param>
        /// <returns>Returns the AzureSearch field name that matches the member.</returns>
        string GetFieldName(Type type, MemberExpression memberExpression);

        /// <summary>
        /// Gets criteria that can be used to find documents of a particular type. Will be used by
        /// AzureSearchLINQ when a query does not have any suitable Where or Query criteria, so that it
        /// can unambiguously select documents of the given type. Typically this should return an 
        /// ExistsCriteria for a field that's known to always have a value.
        /// </summary>
        /// <param name="type">The CLR type that's being searched.</param>
        /// <returns>The criteria for selecting documents of this type.</returns>
        ICriteria GetTypeSelectionCriteria(Type type);

        /// <summary>
        /// Materialize the JObject document object from AzureSearch to a CLR object.
        /// </summary>
        /// <param name="sourceDocument">Source document.</param>
        /// <param name="sourceType">Type of CLR object to materialize to.</param>
        /// <returns>Freshly materialized CLR object version of the source document.</returns>
        object Materialize(Document sourceDocument, Type sourceType);
    }
}
