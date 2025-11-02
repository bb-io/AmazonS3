using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.Actions;

[ActionList("Folders")]
public class FolderActions (InvocationContext invocationContext) : AmazonInvocable(invocationContext)
{
    [Action("Create folder", Description = "Create a folder in an S3 bucket.")]
    public async Task<FolderResponse> CreateFolder(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] CreateFolderRequest folderRequest)
    {
        bucket.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        var createFolderRequest = new PutObjectRequest
        {
            BucketName = bucket.BucketName!,
            Key = folderRequest.GetKey(),
            ContentBody = string.Empty,
        };

        var client = await CreateBucketClient(bucket.BucketName!);
        await ExecuteAction(() => client.PutObjectAsync(createFolderRequest));

        return new FolderResponse { FolderId = folderRequest.GetKey() };
    }

    [Action("Delete folder", Description = "Delete a folder in an S3 bucket.")]
    public async Task DeleteFolder(
        [ActionParameter] BucketRequest bucket,
        [ActionParameter] FolderRequest folderRequest)
    {
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
