using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using GetObjectRequest = Amazon.S3.Model.GetObjectRequest;

namespace Apps.AmazonS3.Actions;

[ActionList]
public class BucketActions
{
    #region Get

    [Action("List buckets", Description = "List all user's buckets")]
    public async Task<List<S3Bucket>> ListBuckets(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var client = CreateClient(authenticationCredentialsProviders.ToArray());
        var bucketResponse = await client.ListBucketsAsync();

        return bucketResponse.Buckets;
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

        var client = await CreateBucketClient(authenticationCredentialsProviders.ToArray(), bucketName);
        var response = await client.ListObjectsV2Async(request);

        return response.S3Objects.Select(x => new BucketObject(x)).ToList();
    }

    [Action("Get object", Description = "Get object from a bucket")]
    public async Task<BucketObject> GetObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] GetObjectRequest objectData)
    {
        var request = new GetObjectRequest
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key
        };

        var client = await CreateBucketClient(authenticationCredentialsProviders.ToArray(), objectData.BucketName);

        return new(await client.GetObjectAsync(request));
    }

    #endregion

    #region Put

    [Action("Upload an object", Description = "Upload an object to a bucket")]
    public async Task<PutObjectResponse> UploadObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] UploadObjectRequest uploadData)
    {
        var request = new PutObjectRequest
        {
            BucketName = uploadData.BucketName,
            Key = uploadData.FileName,
            InputStream = new MemoryStream(uploadData.FileContent)
        };

        var client = await CreateBucketClient(authenticationCredentialsProviders.ToArray(), uploadData.BucketName);

        return await client.PutObjectAsync(request);
    }

    #endregion

    #region Utils

    private AmazonS3Client CreateClient(
        AuthenticationCredentialsProvider[] authenticationCredentialsProviders,
        RegionEndpoint? region = default)
    {
        var key = authenticationCredentialsProviders.First(p => p.KeyName == "access_key");
        var secret = authenticationCredentialsProviders.First(p => p.KeyName == "access_secret");

        if (string.IsNullOrEmpty(key.Value) || string.IsNullOrEmpty(secret.Value))
            throw new Exception(ExceptionMessages.CredentialsMissing);

        return new(key.Value, secret.Value, new AmazonS3Config
        {
            RegionEndpoint = region ?? RegionEndpoint.USWest1
        });
    }

    // The client should have the same region as the bucket, so we have to find out
    // bucket's region in the first place and create new client with this region
    private async Task<AmazonS3Client> CreateBucketClient(
        AuthenticationCredentialsProvider[] authenticationCredentialsProviders,
        string bucketName)
    {
        var client = CreateClient(authenticationCredentialsProviders);
        var locationResponse = await client.GetBucketLocationAsync(bucketName);

        var regionEndpoint = RegionEndpoint.GetBySystemName(locationResponse.Location.Value);

        return CreateClient(authenticationCredentialsProviders, regionEndpoint);
    }

    #endregion
}