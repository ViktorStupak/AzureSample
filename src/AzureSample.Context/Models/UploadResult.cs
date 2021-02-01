using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureSample.Context.Models
{
    public class UploadResult : TableEntity
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ProcessAt { get; set; }
    }
}