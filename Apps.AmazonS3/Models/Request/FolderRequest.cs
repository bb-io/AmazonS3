using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using System.Net;

namespace Apps.AmazonS3.Models.Request;

public record FolderRequest
{
    [Display("Folder")]
    [FileDataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; } = string.Empty;
}