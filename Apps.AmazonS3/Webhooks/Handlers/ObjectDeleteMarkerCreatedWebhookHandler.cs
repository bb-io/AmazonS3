using Amazon.S3;
using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Webhooks.Handlers.Base;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.AmazonS3.Webhooks.Handlers;

public class ObjectDeleteMarkerCreatedWebhookHandler : S3WebhookHandler
{
    protected override EventType Event => EventType.ObjectRemovedDeleteMarkerCreated;

    public ObjectDeleteMarkerCreatedWebhookHandler(InvocationContext invocationContext,
        [WebhookParameter] BucketRequestModel bucketRequest) : base(invocationContext, bucketRequest)
    {
    }
}