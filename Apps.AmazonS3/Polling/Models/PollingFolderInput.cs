using Apps.AmazonS3.DataSourceHandlers.Static;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AmazonS3.Polling.Models;

public record PollingFolderInput : FolderRequest
{   
    [Display("Folder relation trigger")]
    [StaticDataSource(typeof(FolderRelationTriggerDataHandler))]
    public string? FolderRelationTrigger { get; set; }
}