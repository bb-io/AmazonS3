using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Request
{
    public class SearchFilesRequest
    {
        [Display("Folder ID (prefix)", Description = "Describe the folder path structure, f.e. /example/")]
        public string? FolderID { get; set; }
    }
}
