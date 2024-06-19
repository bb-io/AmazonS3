using Amazon.S3.Model;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Extensions;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Polling.Models;
using Apps.AmazonS3.Polling.Models.Memory;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.AmazonS3.Polling;

[PollingEventList]
public class PollingList : BaseInvocable
{
    public PollingList(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [PollingEvent("On files updated", "On any files updated")]
    public async Task<PollingEventResponse<DateMemory, ListPollingFilesResponse>> OnFilesUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingFolderRequest pollingFolder)
    {
        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        var client =
            await AmazonClientFactory.CreateS3BucketClient(
                InvocationContext.AuthenticationCredentialsProviders.ToArray(), pollingFolder.BucketName);

        if (pollingFolder.Folder == "/")
            pollingFolder.Folder = string.Empty;
        
        var objects = client.Paginators.ListObjectsV2(new()
        {
            BucketName = pollingFolder.BucketName,
            Prefix = string.IsNullOrWhiteSpace(pollingFolder.Folder)
                ? string.Empty
                : pollingFolder.Folder
        });
        var result = new List<S3Object>();

        await foreach (var s3Object in objects.S3Objects)
        {
            if (s3Object.LastModified > request.Memory.LastInteractionDate && s3Object.Size != default &&
                IsObjectInFolder(s3Object, pollingFolder))
                result.Add(s3Object);
        }

        if (result.Count == 0)
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };

        return new()
        {
            FlyBird = true,
            Memory = new()
            {
                LastInteractionDate = DateTime.UtcNow
            },
            Result = new()
            {
                Files = result.Select(x => new BucketObject(x))
            }
        };
    }

    private bool IsObjectInFolder(S3Object s3Object, PollingFolderRequest folder)
    {
        if (folder.Folder is null)
            return true;

        if ((string.IsNullOrWhiteSpace(folder.FolderRelationTrigger) ||
             folder.FolderRelationTrigger == FolderRelationTrigger.Descendants) && s3Object.Key.Contains(folder.Folder))
            return true;

        if (folder.FolderRelationTrigger == FolderRelationTrigger.Children &&
            s3Object.GetParentFolder() == folder.Folder.Trim('/').Split('/').Last())
            return true;

        return false;
    }
}