using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBCommentGeneratorWeb.Util
{
    public class ShortUrl : TableEntity
    {
        public ShortUrl(string shortVersion, string longVersion)
        {
            this.PartitionKey = shortVersion.Substring(0, 3);
            this.RowKey = shortVersion.Substring(3);

            LongVersion = longVersion;
        }

        public ShortUrl() { }

        public string LongVersion { get; set; }
        internal string ShortVersion { get => PartitionKey + RowKey; }
    }
}
