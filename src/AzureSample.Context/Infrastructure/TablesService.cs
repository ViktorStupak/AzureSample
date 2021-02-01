using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureSample.Context.Models;
using AzureSample.Context.Options;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using Serilog;

namespace AzureSample.Context.Infrastructure
{
    public class TablesService
    {
        private CloudStorageAccount _storageAccount;
        private TablesOptions _options;
        public TablesService(TablesOptions options)
        {
            this._storageAccount = CreateStorageAccountFromConnectionString(options.TablesAccessKey);
            this._options = options;
        }

        public TablesService(IOptionsMonitor<TablesOptions> options):this(options.CurrentValue)
        {
            options.OnChange(value =>
            {
                this._options = value;
                this._storageAccount = CreateStorageAccountFromConnectionString(value.TablesAccessKey);
            });
        }

        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException ex)
            {
                Log.Error("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.",ex);
                throw;
            }
            catch (ArgumentException ex)
            {
                Log.Error("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.", ex);
                throw;
            }

            return storageAccount;
        }

        public static async Task<CloudTable> CreateTableAsync(CloudStorageAccount account, string tableName)
        {
            CloudTableClient tableClient = account.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Log.Information("Created Table named: {0}", tableName);
            }
            else
            {
                Log.Information("Table {0} already exists", tableName);
            }

            return table;
        }

        public async Task<IEnumerable<UploadResult>> GetData()
        {
            CloudTable table = await CreateTableAsync(this._storageAccount, this._options.TablesName);
            try
            {
              return table.CreateQuery<UploadResult>();
            }
            catch (StorageException e)
            {
                Log.Error(e.Message, e);
                throw;
            }
        }
        public async Task<UploadResult> InsertData(UploadResult entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            CloudTable table = await CreateTableAsync(this._storageAccount, this._options.TablesName);
            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                UploadResult insertedCustomer = result.Result as UploadResult;

                if (result.RequestCharge.HasValue)
                {
                    Log.Information("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                Log.Error(e.Message, e);
                throw;
            }
        }
    }
}