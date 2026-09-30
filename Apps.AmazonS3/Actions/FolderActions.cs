using Amazon.S3.Model;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.AmazonS3.Actions;

[ActionList("Folders")]
public class FolderActions (InvocationContext invocationContext) : AmazonInvocable(invocationContext)
{
    [Action("Create folder", Description = "Create a folder in an S3 bucket.")]
    public async Task<FolderResponse> CreateFolder(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter, Display("New folder name")] string folderName,
        [ActionParameter, Display("Parent folder"), FileDataSource(typeof(FolderDataHandler))] string? parentFolderId)
    {
        var normalizedFolderName = folderName?.Trim('/');
        if (string.IsNullOrWhiteSpace(normalizedFolderName))
            throw new PluginMisconfigurationException("Folder name is required.");

        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(parentFolderId))
            segments.Add(parentFolderId.Trim('/'));
        
        segments.Add(normalizedFolderName);

        var newFolderKey = string.Join('/', segments) + '/';

        var createFolderRequest = new PutObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = newFolderKey,
            ContentBody = string.Empty,
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        await ExecuteAction(() => client.PutObjectAsync(createFolderRequest));

        return new FolderResponse { FolderId = newFolderKey };
    }

    [Action("Delete folder", Description = "Delete a folder in an S3 bucket.")]
    public async Task DeleteFolder(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FolderRequest folderRequest)
    {
        if (string.IsNullOrWhiteSpace(folderRequest.FolderId))
            throw new PluginMisconfigurationException("Folder key is required.");

        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var request = new DeleteObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = folderRequest.FolderId,
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        await ExecuteAction(() => client.DeleteObjectAsync(request));
    }
}
