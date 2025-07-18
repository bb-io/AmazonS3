using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.Models.Request;

public record FolderRequest : BucketRequestModel
{
    [Display("Folder ID (folder name or full path)")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; } = string.Empty;

    [Display("Parent folder ID (prefix, will be combined with folder ID if provided)")]
    public string? ParentFolderId { get; set; } = string.Empty;

    public string GetKey()
    {
        var parentFolderId = ParentFolderId?.TrimEnd('/') ?? string.Empty;
        var folderId = FolderId.TrimStart('/');
        var keyParts = new List<string> { parentFolderId, folderId }.Where(part => !string.IsNullOrEmpty(part));
        return string.Join('/', keyParts).Trim('/') + '/';
    }
}