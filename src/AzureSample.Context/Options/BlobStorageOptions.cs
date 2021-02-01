namespace AzureSample.Context.Options
{
    public class BlobStorageOptions
    {
        public const string Position = "BlobStorageOptions";

        public string BlobAccessKey { get; set; }
        public string BlobContainerName { get; set; }
    }
}