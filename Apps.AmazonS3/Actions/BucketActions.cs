using Amazon.S3.Model;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.AmazonS3.Actions;

[ActionList]
public class BucketActions
{
    #region Get

    [Action("List buckets", Description = "List all user's buckets")]
    public async Task<List<Bucket>> ListBuckets(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var client = S3ClientFactory.CreateClient(authenticationCredentialsProviders.ToArray());
        var bucketResponse = await AmazonClientHandler.ExecuteS3Action(() => client.ListBucketsAsync());

        return bucketResponse.Buckets.Select(x => new Bucket(x)).ToList();
    }

    [Action("List objects in a bucket", Description = "List all objects in a specific bucket")]
    public async Task<List<BucketObject>> ListObjectsInBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Bucket name")]
        string bucketName)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = bucketName
        };

        var client = await S3ClientFactory.CreateBucketClient(authenticationCredentialsProviders.ToArray(), bucketName);
        var response = await AmazonClientHandler.ExecuteS3Action(() => client.ListObjectsV2Async(request));

        return response.S3Objects.Select(x => new BucketObject(x)).ToList();
    }

    [Action("Get object", Description = "Get object from a bucket")]
    public async Task<BucketObject> GetObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] ObjectRequestModel objectData)
    {
        var request = new GetObjectRequest
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key
        };

        var client =
            await S3ClientFactory.CreateBucketClient(authenticationCredentialsProviders.ToArray(),
                objectData.BucketName);

        var response = await AmazonClientHandler.ExecuteS3Action(() => client.GetObjectAsync(request));
        return new(response);
    }

    #endregion

    #region Put

    [Action("Upload an object", Description = "Upload an object to a bucket")]
    public async Task<BucketObject> UploadObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] UploadObjectModel uploadData)
    {
        var request = new PutObjectRequest
        {
            BucketName = uploadData.BucketName,
            Key = uploadData.FileName,
            InputStream = new MemoryStream(uploadData.FileContent)
        };

        var client =
            await S3ClientFactory.CreateBucketClient(authenticationCredentialsProviders.ToArray(),
                uploadData.BucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.PutObjectAsync(request));

        return new(uploadData.BucketName, uploadData.FileName);
    }

    [Action("Create a bucket", Description = "Create an S3 bucket.")]
    public async Task<Bucket> CreateBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] BucketRequestModel createData)
    {
        var request = new PutBucketRequest
        {
            BucketName = createData.BucketName,
            UseClientRegion = true,
        };

        var client = S3ClientFactory.CreateClient(authenticationCredentialsProviders.ToArray());

        await AmazonClientHandler.ExecuteS3Action(() => client.PutBucketAsync(request));

        return new(createData.BucketName, DateTime.Now);
    }

    #endregion

    #region Delete

    [Action("Delete a bucket", Description = "Create an S3 bucket.")]
    public async Task DeleteBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Bucket name")]
        string bucketName)
    {
        var request = new DeleteBucketRequest
        {
            BucketName = bucketName,
        };

        var client = await S3ClientFactory.CreateBucketClient(authenticationCredentialsProviders.ToArray(), bucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.DeleteBucketAsync(request));
    }

    [Action("Delete an object", Description = "Delete an object out of the S3 bucket.")]
    public async Task DeleteObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] ObjectRequestModel deleteData)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = deleteData.BucketName,
            Key = deleteData.Key
        };

        var client =
            await S3ClientFactory.CreateBucketClient(authenticationCredentialsProviders.ToArray(),
                deleteData.BucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.DeleteObjectAsync(request));
    }

    #endregion

    #region Utils



    #endregion
}