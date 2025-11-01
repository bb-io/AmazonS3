using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.DataSourceHandlers;

public class BucketDataHandler(InvocationContext invocationContext) : AmazonInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        if (CurrentConnectionType != ConnectionTypes.AllBuckets)
            throw new PluginMisconfigurationException($"Currently selected connection supports only '{ConnectedBucket}' bucket, so you don't need to specify bucket in the action's input. Please, switch to 'All buckets' conection for working with multiple buckets.");

        var bucketResponse = await ExecuteAction(() => S3Client.ListBucketsAsync(cancellationToken));
        
        return bucketResponse.Buckets
            .Where(x => context.SearchString == null ||
                        x.BucketName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreationDate)
            .Take(20)
            .Select(x => new DataSourceItem(x.BucketName, x.BucketName));
    }
}