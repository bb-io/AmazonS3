using Amazon.S3.Model;
using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public record BucketObject([property: Display("Bucket name")] string BucketName,
    [property: Display("File name")] string Key)
{
    #region Properties

    public string ETag { get; init; }
    [Display("Storage class")] public string? StorageClass { get; init; }
    [Display("Last modified")] public DateTime LastModified { get; init; }
    public long Size { get; init; }

    #endregion

    #region Constructors

    public BucketObject(S3Object s3Object) : this(s3Object.BucketName, s3Object.Key)
    {
        ETag = s3Object.ETag;
        StorageClass = s3Object.StorageClass.Value;
        LastModified = s3Object.LastModified;
        Size = s3Object.Size;
    }

    public BucketObject(GetObjectResponse s3Object) : this(s3Object.BucketName, s3Object.Key)
    {
        ETag = s3Object.ETag;
        StorageClass = s3Object.StorageClass?.Value;
        LastModified = s3Object.LastModified;
        Size = s3Object.ContentLength;
    }

    #endregion
};