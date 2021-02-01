using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureSample.Context.Models;
using AzureSample.Context.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AzureSample.Context.Infrastructure
{
    public class BlobStorageService
    {
        private BlobServiceClient _blobServiceClient;
        private string _containerName;

        public BlobStorageService(BlobStorageOptions options)
        {
            _blobServiceClient = new BlobServiceClient(options.BlobAccessKey);
            this._containerName = options.BlobContainerName;
        }

        public BlobStorageService(IOptionsMonitor<BlobStorageOptions> options):this(options.CurrentValue)
        {
            options.OnChange(value =>
            {
                this._containerName = value.BlobContainerName;
                _blobServiceClient = new BlobServiceClient(value.BlobAccessKey);
            });
        }

        public async Task<BlobSaveResult> SaveFile(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(this._containerName);

            if (!await containerClient.ExistsAsync())
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(this._containerName);
            }

            var fileName = DateTime.Now.ToFileTimeUtc() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(file.OpenReadStream(), true);
            return new BlobSaveResult {UploadDate = DateTime.UtcNow, FileName = fileName};
        }

        public async Task<IEnumerable<Person>> ReadFile(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(this._containerName);

            if (!await containerClient.ExistsAsync())
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(this._containerName);
            }

            var blobClient = containerClient.GetBlobClient(fileName);

            // Open the file and upload its data

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            string fileText;
            using (var sr = new StreamReader(download.Content))
            {
                //This allows you to do one Read operation.
                fileText = await sr.ReadToEndAsync();
            }

            var persons = JsonConvert.DeserializeObject<IEnumerable<Person>>(fileText);

            return persons;
        }
    }
}