namespace Apps.AmazonS3.Webhooks.Models.Payload;

public class ObjectPayload
{
    public string Key { get; set; }
    
    public string ETag { get; set; }
}