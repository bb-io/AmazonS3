using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Request;

public record UploadObjectModel : BucketRequestModel
{
    [Display("File name")] public string FileName { get; set; }
    [Display("File content")] public byte[] FileContent { get; set; }
}