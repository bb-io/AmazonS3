using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AmazonS3.Models.Response
{
    public class DownloadFilesResponse
    {
        [Display("Files")]
        public List<FileReference> Files { get; set; } = new();

        [Display("Total files")]
        public int TotalFiles { get; set; }
    }
}
