using Apps.AmazonS3.Models.Response;

namespace Apps.AmazonS3.Polling.Models;

public class ListPollingFilesResponse
{
    public IEnumerable<BucketObject> Files { get; set; }
}