using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Webhooks.Handlers.Base;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using EventType = Amazon.S3.EventType;

namespace Apps.AmazonS3.Webhooks.Handlers;

public class ObjectRestoreCompletedWebhookHandler(InvocationContext invocationContext, [WebhookParameter] BucketRequestModel bucketRequest)
    : S3WebhookHandler(invocationContext, bucketRequest)
{
    protected override EventType Event => EventType.ObjectRestoreCompleted;
}