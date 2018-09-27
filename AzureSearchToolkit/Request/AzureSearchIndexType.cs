using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Request
{
    public enum AzureSearchIndexType
    {
        Delete = 1,
        Merge = 2,
        MergeOrUpload = 3,
        Upload = 4,
        Unknown = 0
    }
}
