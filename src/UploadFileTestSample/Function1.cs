using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UploadFileTestSample
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([QueueTrigger("queuetemp", Connection = "ConnectionStrings:AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            FunctionUploadFile function = new FunctionUploadFile(log, config);
            function.Run(myQueueItem);
        }
    }
}
