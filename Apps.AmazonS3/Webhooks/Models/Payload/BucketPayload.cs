namespace Apps.AmazonS3.Webhooks.Models.Payload;

public class BucketPayload
{
    public string Name { get; set; } = string.Empty;

    public string Arn { get; set; } = string.Empty;
}