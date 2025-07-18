using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Webhooks.Models.Response;

public class RecordResponse
{
    [Display("Bucket name")]
    public string BucketName { get; set; } = string.Empty;

    [Display("Bucket ARN")]
    public string BucketArn { get; set; } = string.Empty;

    [Display("Key")]
    public string ObjectKey { get; set; } = string.Empty;

    [Display("File ETag")]
    public string ObjectETag { get; set; } = string.Empty;
}