using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public class FileUploadResponse
{
    [Display("File key")]
    public string FileId { get; set; } = string.Empty;

    // ETag is wrapped in double quotes, that's expected behavior and no need to remove them
    // See an issue for details: https://github.com/aws/aws-sdk-net/issues/815
    public string ETag { get; set; } = string.Empty;

    [Display("Bucket name")]
    public string BucketName { get; set; } = string.Empty;
}
