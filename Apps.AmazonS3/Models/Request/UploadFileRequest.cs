using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.AmazonS3.Models.Request;

public record UploadFileRequest : IUploadFileInput
{
    public FileReference File { get; set; } = new();

    [Display("File ID (key)", Description = "Defaults to filename when omitted, can include full path")]
    public string? FileId { get; set; }

    [Display("Folder ID (prefix)", Description = "Defaults to bucket root when omitted")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; } = string.Empty;

    [Display("File metadata")]
    public string? ObjectMetadata { get; set; }
}