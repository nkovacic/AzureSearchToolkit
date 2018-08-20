using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearch.Linq
{
    public interface IAzureSearchQuery<out T> : IOrderedQueryable<T>
    {
    }
}
