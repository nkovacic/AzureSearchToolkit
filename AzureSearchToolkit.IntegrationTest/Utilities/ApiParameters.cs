using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    public class ApiParameters
    {
        private List<string> _parsedFields;
        public Guid? Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Fields { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string Query { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string QueryBy { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; }

        [Range(1, 100)]
        public int Limit { get; set; }

        public bool Summary { get; set; }
        public string Order { get; set; }
        public string OrderBy { get; set; }
        public bool? Pagination { get; set; }
        public bool PrettyPrint { get; set; }

        public List<string> ParsedFields
        {
            get
            {
                if (_parsedFields == null)
                {
                    if (!string.IsNullOrWhiteSpace(Fields))
                    {
                        _parsedFields = Fields.Split(',').ToList();
                    }
                    else
                    {
                        _parsedFields = new List<string>();
                    }
                }

                return _parsedFields;
            }
        }

        public ApiParameters()
        {
            Page = 1;
            Limit = 40;
            Order = "DESC";
            OrderBy = "createdAt";
            PrettyPrint = false;
        }

        public ApiParameters(ApiParameters apiParameters)
        {
            Id = apiParameters.Id;
            ParentId = apiParameters.ParentId;
            Fields = apiParameters.Fields;
            Query = apiParameters.Query;
            QueryBy = apiParameters.QueryBy;
            Page = apiParameters.Page;
            Limit = apiParameters.Limit;
            Summary = apiParameters.Summary;
            Order = apiParameters.Order;
            OrderBy = apiParameters.OrderBy;
            Pagination = apiParameters.Pagination;
            PrettyPrint = apiParameters.PrettyPrint;
        }

        public int StartItem()
        {
            return (Page - 1) * Limit;
        }

        public int EndItem()
        {
            return StartItem() + Limit - 1;
        }

        public void AddField(string field)
        {
            if (!ParsedFields.Any(parsedField => parsedField.Contains(field)))
            {
                ParsedFields.Add(field);
            }
        }

        public bool DoesSearchQueryContain(string queryBy)
        {
            if (IsSearchQuery())
            {
                return QueryBy.IndexOf(queryBy, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        public bool DoesSearchQueryContain(string query, string queryBy)
        {
            if (IsSearchQuery())
            {
                return Query.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                    && QueryBy.IndexOf(queryBy, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        public bool FieldExists(string field)
        {
            return !string.IsNullOrWhiteSpace(Fields) && Fields.Contains(field);
        }

        public List<string> GetSplittedQuery(string delimiter = ",")
        {
            if (IsSearchQuery())
            {
                return Query
                    .Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(q => q.Trim())
                    .ToList();
            }

            return new List<string>();
        }

        public List<string> GetSplittedQueryBy(string delimiter = ",")
        {
            if (IsSearchQuery())
            {
                return QueryBy
                    .Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(q => q.Trim())
                    .ToList();
            }

            return new List<string>();
        }

        public bool HasFields()
        {
            return !string.IsNullOrWhiteSpace(Fields);
        }

        public bool IsSearchQuery()
        {
            return !string.IsNullOrWhiteSpace(Query) && !string.IsNullOrWhiteSpace(QueryBy);
        }

        public bool IsOrderByDefault()
        {
            return string.IsNullOrWhiteSpace(OrderBy) || OrderBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsOrderByRelevance()
        {
            return "relevance".Equals(OrderBy, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsOrderDescending()
        {
            if (string.IsNullOrWhiteSpace(Order))
            {
                return true;
            }

            return Order.Equals("desc", StringComparison.OrdinalIgnoreCase);
        }

        public void RemoveQuery(string property = null)
        {
            if (string.IsNullOrWhiteSpace(property) || GetSplittedQuery().Count <= 1)
            {
                Query = null;
                QueryBy = null;
            }
            else
            {
                var splittedQuery = GetSplittedQuery();
                var indexOfProperty = splittedQuery.IndexOf(property);

                if (indexOfProperty != -1)
                {
                    splittedQuery.RemoveAt(indexOfProperty);

                    Query = string.Join(",", splittedQuery.ToArray());

                    if (!splittedQuery.Any())
                    {
                        QueryBy = null;
                    }
                    else
                    {
                        var splittedQueryBy = GetSplittedQueryBy();

                        if (splittedQueryBy.Count == splittedQuery.Count - 1)
                        {
                            splittedQueryBy.RemoveAt(indexOfProperty);
                        }

                        QueryBy = string.Join(",", splittedQueryBy.ToArray());
                    }
                }
            }
        }
    }
}
