using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.DataSourceHandlers;

public class BucketDataHandler(InvocationContext invocationContext) : AmazonInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var bucketResponse = await ExecuteAction(() => S3Client.ListBucketsAsync(cancellationToken));
        
        return bucketResponse.Buckets
            .Where(x => context.SearchString == null ||
                        x.BucketName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreationDate)
            .Take(20)
            .Select(x => new DataSourceItem(x.BucketName, x.BucketName));
    }
}