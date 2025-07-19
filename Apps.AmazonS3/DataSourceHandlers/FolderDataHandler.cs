using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.DataSourceHandlers;

public class FolderDataHandler(InvocationContext invocationContext, [ActionParameter] BucketRequest bucketRequest)
    : AmazonInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    private readonly BucketRequest _bucketRequest = bucketRequest;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_bucketRequest.BucketName))
            throw new("You should input Bucket name first");

        var client = await CreateBucketClient(_bucketRequest.BucketName);

        var objects = client.Paginators.ListObjectsV2(new()
        {
            BucketName = _bucketRequest.BucketName
        });

        var result = new List<S3Object>();

        await foreach (var s3Object in objects.S3Objects)
            result.Add(s3Object);

        return result
            .Where(x => x.Key.EndsWith('/') && x.Size == default)
            .Where(x => context.SearchString == null ||
                        x.Key.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(30)
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Key))
            .Prepend(new("/", "All files"))
            .Select(x => new DataSourceItem(x.Key, x.Value));
    }
}