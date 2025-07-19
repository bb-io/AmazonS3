using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.DataSourceHandlers.Static;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.Polling.Models;

public record PollingFolderRequest
{
    [Display("Folder ID (prefix)")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; } = string.Empty;

    [Display("Folder relation trigger")]
    [StaticDataSource(typeof(FolderRelationTriggerDataHandler))]
    public string? FolderRelationTrigger { get; set; }
}