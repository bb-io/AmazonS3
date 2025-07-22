using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3.Models.Response;

public class FilesResponse
{
    [Display("Files")]
    public IEnumerable<FileResponse> Files { get; set; } = [];
}
