using Amazon.S3.Model;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Polling.Models.Memory;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;

namespace Apps.AmazonS3.Polling;

[PollingEventList]
public class PollingList(InvocationContext invocationContext) : AmazonInvocable(invocationContext)
{
    [BlueprintEventDefinition(BlueprintEvent.FilesCreatedOrUpdated)]
    [PollingEvent("On files updated", "On any file created or updated")]
    public async Task<PollingEventResponse<DateMemory, FilesResponse>> OnFilesUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] BucketRequest bucket,
        [PollingEventParameter] SearchFilesRequest folderRequest)
    {
        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new() { LastInteractionDate = DateTime.UtcNow }
            };
        }

        if (string.IsNullOrWhiteSpace(folderRequest.FolderId) || folderRequest.FolderId == "/")
            folderRequest.FolderId = string.Empty;

        var result = new List<S3Object>();

        try
        {
            var client = await CreateBucketClient(bucket.BucketName);
            var objects = client.Paginators.ListObjectsV2(new()
            {
                BucketName = bucket.BucketName,
                Prefix = folderRequest.FolderId
            });


            await foreach (var s3Object in objects.S3Objects)
            {
                if (request.Memory.LastInteractionDate > s3Object.LastModified)
                    continue;

                if (s3Object.Key.EndsWith('/') && s3Object.Size == 0)
                    continue;

                if (!ObjectUtils.IsObjectInFolder(s3Object, folderRequest))
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
            Result = new() { Files = result.Select(x => new FileResponse(x)) }
        };
    }
}
