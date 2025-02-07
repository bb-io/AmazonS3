using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AmazonS3.Models.Request;

public record UploadObjectModel : BucketRequestModel
{
    public FileReference File { get; set; }

    [Display("File metadata")]
    public string? ObjectMetadata { get; set; }

    [Display("Key", Description = "Defaults to filename when omitted")]
    public string? Key { get; set; }
}