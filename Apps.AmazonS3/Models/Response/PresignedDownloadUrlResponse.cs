using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public class PresignedDownloadUrlResponse
{
    [Display("Pre-signed URL")]
    public string Url { get; set; } = string.Empty;

    [Display("Expires at")]
    public DateTime ExpiresAt { get; set; }
}
