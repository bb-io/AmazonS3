using Amazon.S3.Model;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Polling.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.DataSourceHandlers;

public class FolderDataHandler: AmazonInvocable, IAsyncDataSourceItemHandler
{
    private readonly PollingFolderRequest _pollingFolderRequest;

    public FolderDataHandler(InvocationContext invocationContext, [ActionParameter] PollingFolderRequest pollingFolderRequest) : base(
        invocationContext)
    {
        _pollingFolderRequest = pollingFolderRequest;
    }

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_pollingFolderRequest.BucketName))
            throw new("You should input Bucket name first");

        var client = await CreateBucketClient(_pollingFolderRequest.BucketName);

        var objects = client.Paginators.ListObjectsV2(new()
        {
            BucketName = _pollingFolderRequest.BucketName
        });

        var result = new List<S3Object>();

        await foreach (var s3Object in objects.S3Objects)
            result.Add(s3Object);

        return result
            .Where(x => x.Key.EndsWith("/") && x.Size == default)
            .Where(x => context.SearchString == null ||
                        x.Key.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(30)
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Key))
            .Prepend(new("/", "All files"))
            .Select(x => new DataSourceItem(x.Key, x.Value));
    }
}