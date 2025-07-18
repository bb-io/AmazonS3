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
    [Action("Create folder", Description = "Create a folder in a bucket")]
    public async Task<FolderResponse> CreateFolder([ActionParameter] FolderRequest createFolderInput)
    {
        var createFolderRequest = new PutObjectRequest
        {
            BucketName = createFolderInput.BucketName,
            Key = createFolderInput.GetKey(),
            ContentBody = string.Empty,
        };

        var client = await CreateBucketClient(createFolderInput.BucketName);
        await ExecuteAction(() => client.PutObjectAsync(createFolderRequest));

        return new FolderResponse { FolderId = createFolderInput.GetKey() };
    }

    [Action("Delete folder", Description = "Delete a folder from a bucket")]
    public async Task DeleteFolder([ActionParameter] FolderRequest deleteFolderInput)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = deleteFolderInput.BucketName,
            Key = deleteFolderInput.GetKey(),
        };

        var client = await CreateBucketClient(deleteFolderInput.BucketName);
        await ExecuteAction(() => client.DeleteObjectAsync(request));
    }
}
