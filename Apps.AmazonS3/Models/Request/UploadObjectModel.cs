using Apps.AmazonS3.Models.Request.Base;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.AmazonS3.Models.Request;

public record UploadObjectModel : BucketRequestModel
{
    public File File { get; set; }
}