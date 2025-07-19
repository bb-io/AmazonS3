using Amazon.S3.Model;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.AmazonS3.Models.Response;

public class FileResponse(S3Object s3Object) : IDownloadFileInput
{
    [Display("File ID (key)")]
    public string FileId { get; set; } = s3Object.Key;

    [Display("Last modified")]
    public DateTime? LastModified { get; init; } = s3Object.LastModified;

    // ETag is wrapped in double quotes, that's expected behavior and no need to remove them
    // See an issue for details: https://github.com/aws/aws-sdk-net/issues/815
    public string ETag { get; init; } = s3Object.ETag;

    [Display("Storage class")]
    public string? StorageClass { get; init; } = s3Object.StorageClass.Value;

    [Display("Bucket name")]
    public string BucketName { get; init; } = s3Object.BucketName;
}