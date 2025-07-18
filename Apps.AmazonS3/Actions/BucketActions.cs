using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.Actions;

[ActionList("Buckets")]
public class BucketActions(InvocationContext invocationContext) : AmazonInvocable(invocationContext)
{
    [Action("Create bucket", Description = "Create an S3 bucket.")]
    public async Task CreateBucket([ActionParameter] [Display("Bucket name")] string bucketName)
    {
        var request = new PutBucketRequest
        {
            BucketName = bucketName,
            UseClientRegion = true,
        };

        await ExecuteAction(() => S3Client.PutBucketAsync(request));
    }

    [Action("Delete bucket", Description = "Delete an S3 bucket.")]
    public async Task DeleteBucket([ActionParameter] BucketRequestModel bucket)
    {
        var request = new DeleteBucketRequest
        {
            BucketName = bucket.BucketName,
        };

        var client = await CreateBucketClient(bucket.BucketName);
        await ExecuteAction(() => client.DeleteBucketAsync(request));
    }
}