using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using ListObjectsRequest = Apps.AmazonS3.Models.Request.ListObjectsRequest;

namespace Apps.AmazonS3.Actions;

[ActionList]
public class BucketActions(InvocationContext invocationContext) : AmazonInvocable(invocationContext)
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

        var response = await AmazonClientHandler.ExecuteS3Action(() => client.GetObjectAsync(request));

        var downloadFileUrl = client.GetPreSignedURL(new()
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key,
            Expires = DateTime.Now.AddHours(1)
        });

        var file = new FileReference(new(HttpMethod.Get, downloadFileUrl), response.Key, response.Headers.ContentType);
        return new(response, file);
    }

    #endregion

    #region Put

    [Action("Upload an object", Description = "Upload an object to a bucket")]
    public async Task UploadObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] UploadObjectModel uploadData)
    {
        var fileStream = await fileManagementClient.DownloadAsync(uploadData.File);
        var request = new PutObjectRequest
        {
            BucketName = uploadData.BucketName,
            Key = uploadData.File.Name,
            InputStream = fileStream,
            Headers = { ContentLength = uploadData.File.Size },
            ContentType = uploadData.File.ContentType
        };

        if (!string.IsNullOrEmpty(uploadData.ObjectMetadata))
        {
            request.Metadata.Add("object", uploadData.ObjectMetadata);
        }

        var client =
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                uploadData.BucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.PutObjectAsync(request));
    }

    [Action("Create a bucket", Description = "Create an S3 bucket.")]
    public async Task<Bucket> CreateBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Bucket name")]
        string bucketName)
    {
        var request = new PutBucketRequest
        {
            BucketName = bucketName,
            UseClientRegion = true,
        };

        var client = AmazonClientFactory.CreateS3Client(authenticationCredentialsProviders.ToArray());

        await AmazonClientHandler.ExecuteS3Action(() => client.PutBucketAsync(request));

        return new(bucketName, DateTime.Now);
    }

    #endregion

    #region Delete

    [Action("Delete a bucket", Description = "Create an S3 bucket.")]
    public async Task DeleteBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] BucketRequestModel bucket)
    {
        var request = new DeleteBucketRequest
        {
            BucketName = bucket.BucketName,
        };

        var client =
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                bucket.BucketName);

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
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                deleteData.BucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.DeleteObjectAsync(request));
    }

    #endregion
}