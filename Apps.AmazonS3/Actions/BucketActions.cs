using Amazon.S3.Model;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.Actions;

[ActionList("Buckets")]
public class BucketActions(InvocationContext invocationContext) : AmazonInvocable(invocationContext)
{
    [Action("Create bucket", Description = "Create a new S3 bucket.")]
    public async Task<BucketResponse> CreateBucket([ActionParameter] [Display("Bucket name")] string bucketName)
    {
        if (CurrentConnectionType != ConnectionTypes.AllBuckets)
            throw new PluginMisconfigurationException($"Currently selected connection supports only '{ConnectedBucket}' bucket. Please, switch to 'All buckets' conection for working with buckets themselves.");

        var request = new PutBucketRequest
        {
            BucketName = bucketName,
            UseClientRegion = true,
        };

        await ExecuteAction(() => S3Client.PutBucketAsync(request));

        return new() { BucketName = bucketName };
    }

    [Action("Delete bucket", Description = "Delete an existing S3 bucket.")]
    public async Task DeleteBucket([ActionParameter] BucketRequest bucket)
    {
        if (CurrentConnectionType != ConnectionTypes.AllBuckets)
            throw new PluginMisconfigurationException($"Currently selected connection supports only '{ConnectedBucket}' bucket. Please, switch to 'All buckets' conection for working with buckets themselves.");

        var request = new DeleteBucketRequest
        {
            BucketName = bucket.BucketName!,
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        await ExecuteAction(() => client.DeleteBucketAsync(request));
    }
}