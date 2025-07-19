using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Utils;
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
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] SearchFilesRequest searchRequest)
    {
        if (string.IsNullOrWhiteSpace(searchRequest.FolderId) || searchRequest.FolderId == "/")
            searchRequest.FolderId = string.Empty;

        var request = new ListObjectsV2Request()
        {
            BucketName = bucket.BucketName,
            Prefix = searchRequest.FolderId,
        };

        var client = await CreateBucketClient(bucket.BucketName);

        var result = await ExecuteAction(async () =>
        {
            var result = new List<FileResponse>();
            await foreach (var s3Object in client.Paginators.ListObjectsV2(request).S3Objects)
            {
                if (s3Object.Key.EndsWith('/') && s3Object.Size == 0)
                    continue;

                if (!ObjectUtils.IsObjectInFolder(s3Object, searchRequest))
                    continue;

                result.Add(new FileResponse(s3Object));
            }
            return result;
        });

        return new FilesResponse { Files = result };
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
    [Action("Download file", Description = "Download a file from a bucket")]
    public async Task<DownloadFileResponse> GetObject(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FileRequest fileRequest)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucket.BucketName,
            Key = fileRequest.FileId,
        };

        var client = await CreateBucketClient(bucket.BucketName);
        var response = await ExecuteAction(() => client.GetObjectAsync(request));

        var fileName = response.Key.Contains('/') ? response.Key.Substring(response.Key.LastIndexOf('/') + 1) : response.Key;

        var downloadFileUrl = client.GetPreSignedURL(new()
        {
            BucketName = bucket.BucketName,
            Key = fileRequest.FileId,
            Expires = DateTime.Now.AddHours(1)
        });        

        var file = new FileReference(new(HttpMethod.Get, downloadFileUrl), fileName, response.Headers.ContentType);
        return new(response, file);
    }

    [BlueprintActionDefinition(BlueprintAction.UploadFile)]
    [Action("Upload file", Description = "Upload a file to a bucket")]
    public async Task UploadObject(
        [ActionParameter] BucketRequest bucket, 
        [ActionParameter] UploadFileRequest uploadRequest)
    {
        var fileStream = await fileManagementClient.DownloadAsync(uploadRequest.File);
        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream);

        var folderId = uploadRequest.FolderId?.TrimEnd('/') ?? string.Empty;
        var fileId = uploadRequest.FileId?.TrimStart('/') ?? uploadRequest.File.Name;
        var keyParts = new List<string> { folderId, fileId }.Where(part => !string.IsNullOrEmpty(part));
        var key = string.Join('/', keyParts).TrimStart('/');

        var request = new PutObjectRequest
        {
            BucketName = bucket.BucketName,
            Key = key,
            InputStream = memoryStream,
            Headers = { ContentLength = memoryStream.Length },
            ContentType = uploadRequest.File.ContentType
        };

        if (!string.IsNullOrEmpty(uploadRequest.ObjectMetadata))
        {
            request.Metadata.Add("object", uploadRequest.ObjectMetadata);
        }

        var client = await CreateBucketClient(bucket.BucketName);
        await ExecuteAction(() => client.PutObjectAsync(request));
    }

    [Action("Delete file", Description = "Delete a file from the S3 bucket.")]
    public async Task DeleteObject(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FileRequest fileRequest)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucket.BucketName,
            Key = fileRequest.FileId,
        };

        var client = await CreateBucketClient(bucket.BucketName);
        await ExecuteAction(() => client.DeleteObjectAsync(request));
    }
}
