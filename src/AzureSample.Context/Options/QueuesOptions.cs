namespace AzureSample.Context.Options
{
    public class QueuesOptions
    {
        public const string Position = "QueuesOptions";

        public string QueuesAccessKey { get; set; }
        public string QueueName { get; set; }
    }
}