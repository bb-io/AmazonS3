using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Request.Base;

public record BucketRequestModel
{
    [Display("Bucket name")] 
    public string BucketName { get; set; } = string.Empty;
}