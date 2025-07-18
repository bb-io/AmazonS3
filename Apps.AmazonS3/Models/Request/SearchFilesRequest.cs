using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.Models.Request;

public class SearchFilesRequest
{
    [Display("Folder ID (prefix)", Description = "Describe the folder path structure, f.e. /example/")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderID { get; set; }
}
