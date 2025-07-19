using Amazon.S3.Model;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.AmazonS3.Models.Response;

public class DownloadFileResponse(GetObjectResponse s3Object, FileReference file) : IDownloadFileOutput
{
    public FileReference File { get; set; } = file;

    [Display("File ID (key)")]
    public string FileId { get; init; } = s3Object.Key;

    [Display("Last modified")]
    public DateTime LastModified { get; init; } = s3Object.LastModified;

    public string ETag { get; init; } = s3Object.ETag;

    [Display("Storage class")]
    public string? StorageClass { get; init; } = s3Object.StorageClass?.Value;

    [Display("Bucket name")]
    public string BucketName { get; init; } = s3Object.BucketName;
}