using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public class BucketResponse
{
    [Display("Bucket name")]
    public string BucketName { get; set; } = string.Empty;
}