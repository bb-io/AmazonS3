using Amazon.S3.Model;
using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public record BucketObject
{
    #region Properties

    [Display("Bucket name")] public string BucketName { get; init; }
    public string ETag { get; init; }
    [Display("File name")] public string Key { get; init; }
    [Display("Storage class")] public string? StorageClass { get; init; }
    [Display("Last modified")] public DateTime LastModified { get; init; }
    public long Size { get; init; }

    #endregion

    #region Constructors

    public BucketObject(S3Object s3Object)
    {
        BucketName = s3Object.BucketName;
        ETag = s3Object.ETag;
        Key = s3Object.Key;
        StorageClass = s3Object.StorageClass.Value;
        LastModified = s3Object.LastModified;
        Size = s3Object.Size;
    }

    public BucketObject(GetObjectResponse s3Object)
    {
        BucketName = s3Object.BucketName;
        ETag = s3Object.ETag;
        Key = s3Object.Key;
        StorageClass = s3Object.StorageClass?.Value;
        LastModified = s3Object.LastModified;
        Size = s3Object.ContentLength;
    }

    #endregion
};