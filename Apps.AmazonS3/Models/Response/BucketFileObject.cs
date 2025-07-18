using Amazon.S3.Model;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.AmazonS3.Models.Response;

public record BucketFileObject : IDownloadFileOutput
{
    public FileReference File { get; set; }

    [Display("Bucket name")]
    public string BucketName { get; init; }

    [Display("Key")]
    public string Key { get; init; }

    public string ETag { get; init; }

    [Display("Storage class")]
    public string? StorageClass { get; init; }

    [Display("Last modified")]
    public DateTime LastModified { get; init; }

    public BucketFileObject(GetObjectResponse s3Object, FileReference file)
    {
        ETag = s3Object.ETag;
        StorageClass = s3Object.StorageClass?.Value;
        LastModified = s3Object.LastModified;
        File = file;
        BucketName = s3Object.BucketName;
        Key = s3Object.Key;
    }
}