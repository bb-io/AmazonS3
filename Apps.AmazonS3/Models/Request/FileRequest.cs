using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.AmazonS3.Models.Request;

public record FileRequest : IDownloadFileInput
{
    [Display("File ID (key)")]
    public string FileId { get; set; } = string.Empty;
}