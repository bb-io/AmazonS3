namespace Apps.AmazonS3.Webhooks.Models.Payload;

public class S3WebhookPayload
{
    public IEnumerable<AmazonRecordPayload> Records { get; set; }
}