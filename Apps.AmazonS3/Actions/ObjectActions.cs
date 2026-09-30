using Amazon.S3.Model;
using Amazon.S3;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.DataSourceHandlers.Static;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.AmazonS3.Actions;

[ActionList("Files")]
public class ObjectActions (InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AmazonInvocable(invocationContext)
{
    private const int MinimumPresignedUrlExpirationMinutes = 1;
    private const int MaximumPresignedUrlExpirationMinutes = 7 * 24 * 60;

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
        if (string.IsNullOrWhiteSpace(fileRequest.FileId))
            throw new PluginMisconfigurationException("File key is required.");

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

    [Action("Generate pre-signed download URL", Description = "Generate a temporary URL for downloading a private S3 object.")]
    public async Task<PresignedDownloadUrlResponse> GeneratePresignedDownloadUrl(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FileRequest fileRequest,
        [ActionParameter, Display("Expiration time (minutes)", Description = "How long the URL remains valid. Must be between 1 and 10080 minutes (7 days).")]
        int expirationMinutes)
    {
        if (string.IsNullOrWhiteSpace(fileRequest.FileId))
            throw new PluginMisconfigurationException("File key is required.");

        if (expirationMinutes is < MinimumPresignedUrlExpirationMinutes or > MaximumPresignedUrlExpirationMinutes)
            throw new PluginMisconfigurationException(
                $"Expiration time must be between {MinimumPresignedUrlExpirationMinutes} and {MaximumPresignedUrlExpirationMinutes} minutes.");

        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket.BucketName!,
            Key = fileRequest.FileId,
            Verb = HttpVerb.GET,
            Expires = expiresAt
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        var url = await ExecuteAction(() => client.GetPreSignedURLAsync(request));

        return new PresignedDownloadUrlResponse
        {
            Url = url,
            ExpiresAt = expiresAt
        };
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

        var folderId = string.IsNullOrWhiteSpace(folder?.FolderId) ? string.Empty : folder.FolderId.TrimEnd('/');
        var fileId = string.IsNullOrWhiteSpace(uploadRequest.FileId)
            ? uploadRequest.File.Name
            : uploadRequest.FileId.TrimStart('/');
        var keyParts = new List<string> { folderId, fileId }.Where(part => !string.IsNullOrEmpty(part));
        var key = string.Join('/', keyParts).TrimStart('/');

        if (string.IsNullOrWhiteSpace(key))
            throw new PluginMisconfigurationException("File key is required.");

        var fileStream = await fileManagementClient.DownloadAsync(uploadRequest.File);
        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream); 
        memoryStream.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = key,
            InputStream = memoryStream,
            Headers = { ContentLength = memoryStream.Length },
            ContentType = uploadRequest.File.ContentType,
            DisableDefaultChecksumValidation = CurrentConnectionType == "S3 Compatible storage" ? IsGcp() : false,     // GCP does not support AWS checksums and returns 400 error
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
        if (string.IsNullOrWhiteSpace(fileRequest.FileId))
            throw new PluginMisconfigurationException("File key is required.");

        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var request = new DeleteObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = fileRequest.FileId,
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        await ExecuteAction(() => client.DeleteObjectAsync(request));
    }

    private bool IsGcp()
    {
        string serviceUrl = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.ServiceUrl).Value;
        return serviceUrl.Contains("googleapis.com", StringComparison.OrdinalIgnoreCase);
    }
}
