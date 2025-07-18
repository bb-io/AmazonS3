using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.AmazonS3.Models.Request.Base;

public record FileInput : BucketRequestModel, IDownloadFileInput
{
    [Display("File ID (key)")]
    public string FileId { get; set; } = string.Empty;
}