using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Webhooks.Handlers.Base;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using EventType = Amazon.S3.EventType;

namespace Apps.AmazonS3.Webhooks.Handlers;

public class ObjectDeleteMarkerCreatedWebhookHandler(InvocationContext invocationContext, [WebhookParameter] BucketRequest bucketRequest)
    : S3WebhookHandler(invocationContext, bucketRequest)
{
    protected override EventType Event => EventType.ObjectRemovedDeleteMarkerCreated;
}