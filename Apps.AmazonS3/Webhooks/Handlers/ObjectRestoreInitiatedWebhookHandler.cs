using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Webhooks.Handlers.Base;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using EventType = Amazon.S3.EventType;

namespace Apps.AmazonS3.Webhooks.Handlers;

public class ObjectRestoreInitiatedWebhookHandler : S3WebhookHandler
{
    protected override EventType Event => EventType.ObjectRestorePost;

    public ObjectRestoreInitiatedWebhookHandler(InvocationContext invocationContext,
        [WebhookParameter] BucketRequestModel bucketRequest) :
        base(invocationContext, bucketRequest)
    {
    }
}