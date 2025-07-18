using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.AmazonS3.Actions;

[ActionList("Files")]
public class ObjectActions (InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AmazonInvocable(invocationContext)
{
    [Action("Search files", Description = "Search for objects in a specific bucket")]
    public async Task<FilesResponse> ListObjectsInBucket(
        [ActionParameter] BucketRequestModel bucket,
        [ActionParameter] SearchFilesRequest input)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = bucket.BucketName,
            Prefix = input.FolderID ?? string.Empty,
        };

        var client = await CreateBucketClient(bucket.BucketName);

        var result = await ExecuteAction(async () =>
        {
            var result = new List<FileObject>();
            await foreach (var s3Object in client.Paginators.ListObjectsV2(request).S3Objects)
            {
                if(!s3Object.Key.EndsWith('/'))
                {
                    result.Add(new FileObject(s3Object));
                }
            }
            return result;
        });

        return new FilesResponse { Objects = result };
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
    [Action("Download file", Description = "Download a file from a bucket")]
    public async Task<BucketFileObject> GetObject([ActionParameter] FileInput input)
    {
        var request = new GetObjectRequest
        {
            BucketName = input.BucketName,
            Key = input.FileId,
        };

        var client = await CreateBucketClient(input.BucketName);

        var response = await ExecuteAction(() => client.GetObjectAsync(request));

        var downloadFileUrl = client.GetPreSignedURL(new()
        {
            BucketName = input.BucketName,
            Key = input.FileId,
            Expires = DateTime.Now.AddHours(1)
        });
        string fileName = response.Key.Contains('/') ? response.Key.Substring(response.Key.LastIndexOf('/') + 1) : response.Key;

        var file = new FileReference(new(HttpMethod.Get, downloadFileUrl), fileName, response.Headers.ContentType);
        return new(response, file);
    }

    [BlueprintActionDefinition(BlueprintAction.UploadFile)]
    [Action("Upload file", Description = "Upload a file to a bucket")]
    public async Task UploadObject([ActionParameter] UploadFileInput input)
    {
        var fileStream = await fileManagementClient.DownloadAsync(input.File);
        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream);
        var fileLength = memoryStream.Length;

        var folderId = input.FolderId?.TrimEnd('/') ?? string.Empty;
        var fileId = input.Key?.TrimStart('/') ?? input.File.Name;
        var keyParts = new List<string> { folderId, fileId }.Where(part => !string.IsNullOrEmpty(part));
        var key = string.Join('/', keyParts).TrimStart('/');

        var request = new PutObjectRequest
        {
            BucketName = input.BucketName,
            Key = key,
            InputStream = memoryStream,
            Headers = { ContentLength = fileLength },
            ContentType = input.File.ContentType
        };

        if (!string.IsNullOrEmpty(input.ObjectMetadata))
        {
            request.Metadata.Add("object", input.ObjectMetadata);
        }

        var client = await CreateBucketClient(input.BucketName);

        await ExecuteAction(() => client.PutObjectAsync(request));
    }

    [Action("Delete file", Description = "Delete a file from the S3 bucket.")]
    public async Task DeleteObject([ActionParameter] FileInput deleteData)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = deleteData.BucketName,
            Key = deleteData.FileId,
        };

        var client = await CreateBucketClient(deleteData.BucketName);

        await ExecuteAction(() => client.DeleteObjectAsync(request));
    }
}
