using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureSample.Context.Infrastructure;
using AzureSample.Context.Models;
using AzureSample.Context.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace UploadFileTestSample
{
    public class FunctionUploadFile
    {
        private readonly BlobStorageService _blobStorage;
        private readonly DataContext _dataContext;
        private readonly TablesService _tablesService;
        private readonly ILogger _log;
        public FunctionUploadFile(ILogger log, IConfigurationRoot configuration)
        {
            this._log = log;


            BlobStorageOptions blobStorage = new BlobStorageOptions
            {
                BlobAccessKey = configuration["BlobAccessKey"],
                BlobContainerName = configuration["BlobContainerName"]
            };
            _blobStorage = new BlobStorageService(blobStorage);
            TablesOptions tablesOptions = new TablesOptions
            {
                TablesAccessKey = configuration["TablesAccessKey"],
                TablesName = configuration["TablesName"]
            };
            _tablesService = new TablesService(tablesOptions);
            _dataContext = new DataContext(configuration["SqlConnection"]);
        }

        public void Run(string myQueueItem)
        {
            UploadResult result = new UploadResult();
            result.PartitionKey = Guid.NewGuid().ToString();
            result.RowKey = Guid.NewGuid().ToString();
            var blobSaveResult = JsonConvert.DeserializeObject<BlobSaveResult>(myQueueItem);
            result.Message += $"process new file {blobSaveResult.FileName} ";
            IEnumerable<Person> newPersons = new List<Person>();
            try
            {
                newPersons = _blobStorage.ReadFile(blobSaveResult.FileName).Result;

            }
            catch (Exception e)
            {
                this._log.LogError("Some error during read file from blob storage", e);
                result.Message += e.Message;
                result.Success = false;
            }

            try
            {
                foreach (var person in newPersons)
                {
                    _dataContext.Persons.Add(person);
                    _dataContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                this._log.LogError("Some error during save persons to DB", e);
                result.Message += e.Message;
                result.Success = false;
            }

            result.Message += $" insert {newPersons.Count()} new person";
            result.ProcessAt = DateTime.UtcNow;
            result.CreatedAt = blobSaveResult.UploadDate;
            try
            {

                var inserted = _tablesService.InsertData(result).Result;
            }
            catch (Exception e)
            {
                this._log.LogError("Some error during save result to tablesService", e);
                result.Message += e.Message;
            }
            this._log.LogInformation(result.Message);
        }
    }
}
