using Amazon.S3.Model;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Models_Request_ListObjectsRequest = Apps.AmazonS3.Models.Request.ListObjectsRequest;

namespace Apps.AmazonS3.Actions;

[ActionList]
public class BucketActions(IFileManagementClient fileManagementClient)
{
    [Action("List objects in a bucket", Description = "List all objects in a specific bucket")]
    public async Task<List<BucketObject>> ListObjectsInBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] BucketRequestModel bucket, [ActionParameter] Models_Request_ListObjectsRequest input)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = bucket.BucketName
        };

        var client =
            await AmazonClientHandler.ExecuteS3Action(async () => await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                bucket.BucketName));

        var objects = AmazonClientHandler.ExecuteS3Action(() => client.Paginators.ListObjectsV2(request));
        var result = new List<BucketObject>();

        try
        {
            await foreach (var s3Object in objects.S3Objects)
            {
                if (input.IncludeFoldersInResult != null && input.IncludeFoldersInResult == true)
                { 
                    result.Add(new BucketObject(s3Object)); 
                }
                else
                {
                    if (!s3Object.Key.EndsWith("/")  ) 
                    {
                        result.Add(new BucketObject(s3Object));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

        return result;
    }

    [Action("Get object", Description = "Get object from a bucket")]
    public async Task<BucketFileObject> GetObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] ObjectRequestModel objectData)
    {
        var request = new GetObjectRequest
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key
        };

        var client =
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                objectData.BucketName);

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
}