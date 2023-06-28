using Amazon.S3.Model;
using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public record Bucket([property: Display("Bucket name")] string BucketName,
    [property: Display("Creation date")] DateTime CreationDate)
{
    public Bucket(S3Bucket s3Bucket) : this(s3Bucket.BucketName, s3Bucket.CreationDate)
    {
    }
}