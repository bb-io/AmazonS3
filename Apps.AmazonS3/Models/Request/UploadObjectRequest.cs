using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Request;

public record UploadObjectRequest
{
    [Display("Bucket name")] public string BucketName { get; set; }
    [Display("File name")] public string FileName { get; set; }
    [Display("File content")] public byte[] FileContent { get; set; }
}