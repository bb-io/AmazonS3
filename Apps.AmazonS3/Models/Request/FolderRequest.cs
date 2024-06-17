using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.Models.Request;

public class FolderRequest
{
    [Display("Bucket name")] 
    [DataSource(typeof(BucketDataHandler))]
    public string BucketName { get; set; }
    
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string Folder { get; set; }
}