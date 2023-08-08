using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.Models.Request.Base;

public record BucketRequestModel
{
    [Display("Bucket name")] 
    [DataSource(typeof(BucketDataHandler))]
    public string BucketName { get; set; }
}