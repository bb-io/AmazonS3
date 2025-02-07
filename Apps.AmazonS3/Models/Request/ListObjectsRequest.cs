using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Request
{
    public class ListObjectsRequest
    {
        [Display("Include folders in result?")]
        public bool? IncludeFoldersInResult { get; set; }

        [Display("Prefix", Description = "Describe the folder path structure, f.e. /example/")]
        public string? Prefix {  get; set; }
    }
}
