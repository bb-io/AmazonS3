using Amazon.S3;
using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Webhooks.Handlers.Base;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.Webhooks.Handlers;

public class ObjectTagRemovedWebhookHandler : S3WebhookHandler
{
    protected override EventType Event => EventType.S3ObjectTaggingDelete;

    public ObjectTagRemovedWebhookHandler(InvocationContext invocationContext, BucketRequestModel bucketRequest) : base(
        invocationContext, bucketRequest)
    {
    }
}