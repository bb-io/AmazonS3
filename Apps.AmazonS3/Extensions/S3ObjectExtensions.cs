using Amazon.S3.Model;

namespace Apps.AmazonS3.Extensions;

public static class S3ObjectExtensions
{
    public static string GetParentFolder(this S3Object s3Object)
    {
        var keySegments = s3Object.Key.Split("/");
        return keySegments.Length < 2 ? string.Empty : keySegments[^2];
    }
}