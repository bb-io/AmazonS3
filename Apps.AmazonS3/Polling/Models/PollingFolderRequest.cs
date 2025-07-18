using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.Polling.Models;

public class PollingFolderRequest
{
    [Display("Bucket name")] 
    [DataSource(typeof(BucketDataHandler))]
    public string BucketName { get; set; } = string.Empty;

    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string? Folder { get; set; }
    
    [Display("Folder relation trigger")]
    [StaticDataSource(typeof(FolderRelationTriggerDataHandler))]
    public string? FolderRelationTrigger { get; set; }
}