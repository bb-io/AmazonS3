using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AmazonS3.Models.Request;

public record UploadObjectModel : BucketRequestModel
{
    public FileReference File { get; set; }
}