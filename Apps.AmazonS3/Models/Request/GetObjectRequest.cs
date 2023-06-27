using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Request;

public record GetObjectRequest
{
    [Display("Bucket name")] public string BucketName { get; set; }
    [Display("Object key")] public string Key { get; set; }
}