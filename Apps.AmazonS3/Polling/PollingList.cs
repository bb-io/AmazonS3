using Amazon.S3.Model;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Extensions;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Polling.Models;
using Apps.AmazonS3.Polling.Models.Memory;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Blackbird.Applications.SDK.Blueprints;

namespace Apps.AmazonS3.Polling;

[PollingEventList]
public class PollingList(InvocationContext invocationContext) : AmazonInvocable(invocationContext)
{
    [BlueprintEventDefinition(BlueprintEvent.FilesCreatedOrUpdated)]
    [PollingEvent("On files updated", "On any file created or updated")]
    public async Task<PollingEventResponse<DateMemory, FilesResponse>> OnFilesUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingFolderInput input)
    {
        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new() { LastInteractionDate = DateTime.UtcNow }
            };
        }

        if (input.FolderId == "/")
            input.FolderId = string.Empty;

        var result = new List<S3Object>();

        try
        {
            var client = await CreateBucketClient(input.BucketName);
            var objects = client.Paginators.ListObjectsV2(new()
            {
                BucketName = input.BucketName,
                Prefix = string.IsNullOrWhiteSpace(input.FolderId)
                    ? string.Empty
                    : input.FolderId
            });


            await foreach (var s3Object in objects.S3Objects)
            {
                if (request.Memory.LastInteractionDate > s3Object.LastModified)
                    continue;

                if (s3Object.Size == default)
                    continue;

                if (!IsObjectInFolder(s3Object, input))
                    continue;

                result.Add(s3Object);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = "[AmazonS3 polling] Got an error while polling. "
                + $"Method: OnFilesUpdated"
                + $"Exception message: {ex.Message}";

            InvocationContext.Logger?.LogError(errorMessage, [ex.Message]);
            throw;
        }

        if (result.Count == 0)
            return new()
            {
                FlyBird = false,
                Memory = new() { LastInteractionDate = DateTime.UtcNow }
            };

        return new()
        {
            FlyBird = true,
            Memory = new() { LastInteractionDate = DateTime.UtcNow },
            Result = new() { Objects = result.Select(x => new FileObject(x)) }
        };
    }

    private static bool IsObjectInFolder(S3Object s3Object, PollingFolderInput input)
    {
        if (input.FolderId is null)
            return true;

        if ((string.IsNullOrWhiteSpace(input.FolderRelationTrigger) ||
             input.FolderRelationTrigger == FolderRelationTrigger.Descendants) && s3Object.Key.Contains(input.FolderId))
            return true;

        if (input.FolderRelationTrigger == FolderRelationTrigger.Children &&
            s3Object.GetParentFolder() == input.FolderId.Trim('/').Split('/').Last())
            return true;

        return false;
    }
}