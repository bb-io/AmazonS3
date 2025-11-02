using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using System.Net;

namespace Apps.AmazonS3.Models.Request;

public record CreateFolderRequest
{
    private string? _parentFolderId;

    [Display("Folder name")]
    public string FolderName { get; set; } = string.Empty;

    [Display("Parent folder")]
    [FileDataSource(typeof(FolderDataHandler))]
    public string? ParentFolderId
    {
        get => WebUtility.UrlDecode(_parentFolderId);
        set => _parentFolderId = value;
    }

    public string GetKey()
    {
        var parentFolderId = ParentFolderId?.TrimEnd('/') ?? string.Empty;
        var folderId = FolderName.TrimStart('/');
        var keyParts = new List<string> { parentFolderId, folderId }.Where(part => !string.IsNullOrEmpty(part));
        return string.Join('/', keyParts).Trim('/') + '/';
    }
}