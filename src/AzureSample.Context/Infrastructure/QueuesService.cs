using System.Threading.Tasks;
using Azure.Storage.Queues;
using AzureSample.Context.Models;
using AzureSample.Context.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AzureSample.Context.Infrastructure
{
    public class QueuesService
    {
        private QueueClient _queueClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuesService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public QueuesService(IOptionsMonitor<QueuesOptions> options)
        {
            this._queueClient = new QueueClient(options.CurrentValue.QueuesAccessKey, options.CurrentValue.QueueName);
            this._queueClient.CreateIfNotExists();
            options.OnChange(value =>
            {
                this._queueClient = new QueueClient(value.QueuesAccessKey, value.QueueName);
                this._queueClient.CreateIfNotExists();
            });
        }

        /// <summary>
        /// Inserts the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public async Task InsertMessage(BlobSaveResult message)
        {
            var serializeObject = JsonConvert.SerializeObject(message);
            if (await _queueClient.ExistsAsync())
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(serializeObject);
                // Send a message to the queue
                await _queueClient.SendMessageAsync(System.Convert.ToBase64String(plainTextBytes));
            }
        }
    }
}