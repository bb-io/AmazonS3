using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.Models.Request;

public class SearchFilesRequest
{
    [Display("Folder prefix", Description = "Folder path, eg /example/subfolder/")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }

    [Display("Folder relation")]
    [StaticDataSource(typeof(FolderRelationTriggerDataHandler))]
    public string? FolderRelationTrigger { get; set; }
}
