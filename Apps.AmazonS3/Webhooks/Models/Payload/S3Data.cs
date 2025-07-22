namespace Apps.AmazonS3.Webhooks.Models.Payload;

public class S3Data
{
    public BucketPayload Bucket { get; set; } = new();

    public ObjectPayload Object { get; set; } = new();
}