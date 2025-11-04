using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public class FolderResponse
{
    [Display("Folder")]
    public string FolderId { get; set; } = string.Empty;
}