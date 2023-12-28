using Apps.AmazonS3.Actions;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.DataSourceHandlers;

public class BucketDataHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;
    
    public BucketDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = AmazonClientFactory.CreateS3Client(Creds.ToArray());
        var bucketResponse = await AmazonClientHandler.ExecuteS3Action(() => client.ListBucketsAsync(cancellationToken));
        
        return bucketResponse.Buckets
            .Where(x => context.SearchString == null ||
                        x.BucketName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreationDate)
            .Take(20)
            .ToDictionary(x => x.BucketName, x => x.BucketName);
    }
}