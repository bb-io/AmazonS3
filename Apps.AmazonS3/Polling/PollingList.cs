using Amazon.S3.Model;
using Apps.AmazonS3.Extensions;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Models.Request;
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
        [PollingEventParameter] FolderRequest folder)
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
                InvocationContext.AuthenticationCredentialsProviders.ToArray(), folder.BucketName);

        var objects = client.Paginators.ListObjectsV2(new()
        {
            BucketName = folder.BucketName,
            Prefix = folder.Folder is "/" ? string.Empty : folder.Folder
        });
        var result = new List<S3Object>();

        await foreach (var s3Object in objects.S3Objects)
        {
            if (s3Object.LastModified > request.Memory.LastInteractionDate && s3Object.Size != default &&
                s3Object.GetParentFolder() == folder.Folder.Trim('/'))
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
}