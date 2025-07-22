namespace Apps.AmazonS3.Webhooks.Models.Payload;

public class ObjectPayload
{
    public string Key { get; set; } = string.Empty;

    public string ETag { get; set; } = string.Empty;
}