using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Webhooks.Handlers.Base;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using EventType = Amazon.S3.EventType;

namespace Apps.AmazonS3.Webhooks.Handlers;

public class ObjectTagRemovedWebhookHandler : S3WebhookHandler
{
    protected override EventType Event => EventType.S3ObjectTaggingDelete;

    public ObjectTagRemovedWebhookHandler(InvocationContext invocationContext,
        [WebhookParameter] BucketRequestModel bucketRequest) : base(
        invocationContext, bucketRequest)
    {
    }
}