using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace RBBCommentGeneratorWeb.Util
{
    public class StorageClient
    {
        private static AzureStorageSettings _settings = null;

        public static void SetSettings(AzureStorageSettings newSettings)
        {
            _settings = newSettings;
        }

        public static async Task SaveShorten(string shortString, string longString)
        {
            var storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(_settings.AccountName, _settings.KeyValue),
                true);

            var tableClient = storageAccount.CreateCloudTableClient();

            var urlTable = tableClient.GetTableReference("RBBShortUrls");

            var newShort = new ShortUrl(shortString, longString);

            var insertOperation = TableOperation.Insert(newShort);

            await urlTable.ExecuteAsync(insertOperation);
        }

        public static async Task<string> GetShortFor(string longString)
        {
            var storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(_settings.AccountName, _settings.KeyValue),
                true);

            var tableClient = storageAccount.CreateCloudTableClient();

            var urlTable = tableClient.GetTableReference("RBBShortUrls");

            var findFilter = TableQuery.GenerateFilterCondition("LongVersion", QueryComparisons.Equal, longString);

            var query = new TableQuery<ShortUrl>().Where(findFilter);

            TableContinuationToken token = null;

            var result = await urlTable.ExecuteQuerySegmentedAsync(query, token);

            if (result.Results.Count > 0)
            {
                return result.Results[0].PartitionKey + result.Results[0].RowKey;
            }

            return null;
        }

        public static async Task<string> GetForShorten(string shortString)
        {
            var storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(_settings.AccountName, _settings.KeyValue), 
                true);

            var tableClient = storageAccount.CreateCloudTableClient();

            var urlTable = tableClient.GetTableReference("RBBShortUrls");

            var retrieveOperation = TableOperation.Retrieve<ShortUrl>(shortString.Substring(0, 3), shortString.Substring(3));

            var retrieveResult = await urlTable.ExecuteAsync(retrieveOperation);

            var entity = retrieveResult.Result as ShortUrl;

            if (entity != null)
                return entity.LongVersion;

            return null;
        }
    }
}
