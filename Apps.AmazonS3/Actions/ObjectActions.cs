using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.AmazonS3.Actions;

[ActionList]
public class ObjectActions (InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AmazonInvocable(invocationContext)
{
    [Action("Search files in bucket", Description = "Search for objects in a specific bucket")]
    public async Task<FilesResponse> ListObjectsInBucket([ActionParameter] BucketRequestModel bucket, [ActionParameter] Models.Request.ListObjectsRequest input)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = bucket.BucketName,
            Prefix = input.Prefix,
        };

        var client = await CreateBucketClient(bucket.BucketName);

        var result = await ExecuteAction(async () =>
        {
            var result = new List<BucketObject>();
            await foreach (var s3Object in client.Paginators.ListObjectsV2(request).S3Objects)
            {
                if (input.IncludeFoldersInResult != null && input.IncludeFoldersInResult == true)
                {
                    result.Add(new BucketObject(s3Object));
                }
                else
                {
                    if (!s3Object.Key.EndsWith("/"))
                    {
                        result.Add(new BucketObject(s3Object));
                    }
                }
            }
            return result;
        });

        return new FilesResponse { Objects = result };

    }

    [Action("Download file", Description = "Download a file from a bucket")]
    public async Task<BucketFileObject> GetObject([ActionParameter] ObjectRequestModel objectData)
    {
        var request = new GetObjectRequest
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key
        };

        var client = await CreateBucketClient(objectData.BucketName);

        var response = await ExecuteAction(() => client.GetObjectAsync(request));

        var downloadFileUrl = client.GetPreSignedURL(new()
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key,
            Expires = DateTime.Now.AddHours(1)
        });
        string fileName = response.Key.Contains("/") ? response.Key.Substring(response.Key.LastIndexOf('/') + 1) : response.Key;

        var file = new FileReference(new(HttpMethod.Get, downloadFileUrl), fileName, response.Headers.ContentType);
        return new(response, file);
    }

    [Action("Upload file", Description = "Upload a file to a bucket")]
    public async Task UploadObject([ActionParameter] UploadObjectModel uploadData)
    {
        var fileStream = await fileManagementClient.DownloadAsync(uploadData.File);
        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream);
        var fileLength = memoryStream.Length;

        var request = new PutObjectRequest
        {
            BucketName = uploadData.BucketName,
            Key = uploadData.Key ?? uploadData.File.Name,
            InputStream = memoryStream,
            Headers = { ContentLength = fileLength },
            ContentType = uploadData.File.ContentType
        };

        if (!string.IsNullOrEmpty(uploadData.ObjectMetadata))
        {
            request.Metadata.Add("object", uploadData.ObjectMetadata);
        }

        var client = await CreateBucketClient(uploadData.BucketName);

        await ExecuteAction(() => client.PutObjectAsync(request));
    }
}
