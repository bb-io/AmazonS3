namespace Apps.AmazonS3.Webhooks.Models.Response;

public class S3WebhookResponse
{
    public IEnumerable<RecordResponse> Records { get; set; } = [];
}