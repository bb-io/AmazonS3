using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Webhooks.Models.Response;

public class RecordResponse
{
    [Display("Bucket name")] public string BucketName { get; set; }

    [Display("Bucket ARN")] public string BucketArn { get; set; }

    [Display("Object key")] public string ObjectKey { get; set; }

    [Display("Object ETag")] public string ObjectETag { get; set; }
}