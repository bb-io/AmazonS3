using Amazon.S3.Model;
using Apps.AmazonS3.DataSourceHandlers.Static;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.AmazonS3.Actions;

[ActionList("Files")]
public class ObjectActions (InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AmazonInvocable(invocationContext)
{
    [Action("Search files", Description = "Search for files in a specific S3 bucket.")]
    public async Task<FilesResponse> ListObjectsInBucket(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FolderRequest folderRequest,
        [Display("Folder relation"), StaticDataSource(typeof(FolderRelationTriggerDataHandler))] string? folderRelationTrigger)
    {
        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        if (string.IsNullOrWhiteSpace(folderRequest.FolderId) || folderRequest.FolderId == "/")
            folderRequest.FolderId = string.Empty;

        var request = new ListObjectsV2Request()
        {
            BucketName = bucket.BucketName!,
            Prefix = folderRequest.FolderId,
        };

        var client = await CreateBucketClient(bucket.BucketName!);

        var result = await ExecuteAction(async () =>
        {
            var result = new List<FileResponse>();
            await foreach (var s3Object in client.Paginators.ListObjectsV2(request).S3Objects)
            {
                if (s3Object.Key.EndsWith('/') && s3Object.Size == 0)
                    continue;

                if (!ObjectUtils.IsObjectInFolder(s3Object, folderRequest.FolderId, folderRelationTrigger))
                    continue;

                result.Add(new FileResponse(s3Object));
            }
            return result;
        });

        return new FilesResponse { Files = result };
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
    [Action("Download file", Description = "Download a file from an S3 bucket.")]
    public async Task<DownloadFileResponse> GetObject(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FileRequest fileRequest)
    {
        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var request = new GetObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = fileRequest.FileId,
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        var response = await ExecuteAction(() => client.GetObjectAsync(request));

        var fileName = response.Key.Contains('/') ? response.Key.Substring(response.Key.LastIndexOf('/') + 1) : response.Key;

        var downloadFileUrl = client.GetPreSignedURL(new()
        {
            BucketName = bucket.BucketName!,
            Key = fileRequest.FileId,
            Expires = DateTime.Now.AddHours(1)
        });        

        var file = new FileReference(new(HttpMethod.Get, downloadFileUrl), fileName, response.Headers.ContentType);
        return new(response, file);
    }

    [Action("Download all files", Description = "Download all files in a bucket. Optionally restrict to a folder. Returns array of files")]
    public async Task<DownloadFilesResponse> DownloadAllFiles(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] OptionalFolderRequest folder)
    {
        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var prefix = folder?.FolderId;

        if (string.IsNullOrWhiteSpace(prefix) || prefix == "/")
        {
            prefix = string.Empty;
        }
        else
        {
            prefix = prefix.Trim('/');
            prefix = prefix + "/";
        }

        var request = new ListObjectsV2Request
        {
            BucketName = bucket.BucketName!,
            Prefix = prefix
        };

        var client = await CreateBucketClient(bucket.BucketName!);

        var files = await ExecuteAction(async () =>
        {
            var result = new List<FileReference>();

            await foreach (var s3Object in client.Paginators.ListObjectsV2(request).S3Objects)
            {
                if (s3Object.Key.EndsWith('/') && s3Object.Size == 0)
                    continue;

                var entryName = string.IsNullOrEmpty(prefix)
                    ? s3Object.Key
                    : s3Object.Key.StartsWith(prefix)
                        ? s3Object.Key.Substring(prefix.Length).TrimStart('/')
                        : s3Object.Key;

                if (string.IsNullOrWhiteSpace(entryName))
                    continue;

                var contentType = "application/octet-stream";

                try
                {
                    var meta = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                    {
                        BucketName = bucket.BucketName!,
                        Key = s3Object.Key
                    });

                    if (!string.IsNullOrWhiteSpace(meta.Headers.ContentType))
                        contentType = meta.Headers.ContentType;
                }
                catch
                {
                }

                var downloadUrl = client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = bucket.BucketName!,
                    Key = s3Object.Key,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                result.Add(new FileReference(new(HttpMethod.Get, downloadUrl), entryName, contentType));
            }

            return result;
        });

        return new DownloadFilesResponse { Files = files, TotalFiles=files.Count() };
    }
    

    [BlueprintActionDefinition(BlueprintAction.UploadFile)]
    [Action("Upload file", Description = "Upload a file to an S3 bucket.")]
    public async Task<FileUploadResponse> UploadObject(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] OptionalFolderRequest folder,
        [ActionParameter] UploadFileRequest uploadRequest)
    {
        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var fileStream = await fileManagementClient.DownloadAsync(uploadRequest.File);
        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream);

        var folderId = folder is null ? string.Empty : folder.FolderId?.TrimEnd('/');
        var fileId = uploadRequest.FileId?.TrimStart('/') ?? uploadRequest.File.Name;
        var keyParts = new List<string> { folderId!, fileId }.Where(part => !string.IsNullOrEmpty(part));
        var key = keyParts is null ? fileId : string.Join('/', keyParts).TrimStart('/');

        var request = new PutObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = key,
            InputStream = memoryStream,
            Headers = { ContentLength = memoryStream.Length },
            ContentType = uploadRequest.File.ContentType
        };

        if (!string.IsNullOrEmpty(uploadRequest.FileMetadata))
        {
            request.Metadata.Add("object", uploadRequest.FileMetadata);
        }

        var client = await CreateBucketClient(bucket.BucketName!);
        var uploadResult = await ExecuteAction(() => client.PutObjectAsync(request));

        return new()
        {
            FileId = key,
            ETag = uploadResult.ETag,
            BucketName = bucket.BucketName!,
        };
    }

    [Action("Delete file", Description = "Delete a file in an S3 bucket.")]
    public async Task DeleteObject(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FileRequest fileRequest)
    {
        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var request = new DeleteObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = fileRequest.FileId,
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        await ExecuteAction(() => client.DeleteObjectAsync(request));
    }

    private static string NormalizePrefix(string? folderId)
    {
        if (string.IsNullOrWhiteSpace(folderId) || folderId == "/")
            return string.Empty;

        var normalized = folderId.Trim();

        normalized = normalized.Trim('/');

        return string.IsNullOrEmpty(normalized) ? string.Empty : normalized + "/";
    }
}
