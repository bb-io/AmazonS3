using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Request.Base;

public record ObjectRequestModel : BucketRequestModel
{
    [Display("Object key")] public string Key { get; set; }
}