using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Webhooks.Models.Response;

public class RecordResponse
{
    [Display("File or folder ID (key)")]
    public string ObjectKey { get; set; } = string.Empty;

    [Display("ETag")]
    public string ObjectETag { get; set; } = string.Empty;

    [Display("Bucket name")]
    public string BucketName { get; set; } = string.Empty;

    [Display("Bucket ARN")]
    public string BucketArn { get; set; } = string.Empty;
}