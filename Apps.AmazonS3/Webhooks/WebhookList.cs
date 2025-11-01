using Apps.AmazonS3.Webhooks.Handlers;
using Apps.AmazonS3.Webhooks.Models.Payload;
using Apps.AmazonS3.Webhooks.Models.Response;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;

namespace Apps.AmazonS3.Webhooks;

[WebhookList]
public class WebhookList(InvocationContext invocationContext) : AmazonInvocable(invocationContext)
{
    [Webhook("On file or folder created",
        typeof(ObjectCreatedWebhookHandler),
        Description = "Triggered when an object is created in the bucket.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectCreated(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On file or folder delete marker created",
        typeof(ObjectDeleteMarkerCreatedWebhookHandler),
        Description = "Triggered when a delete marker is created for a specific object.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectDeleteMarkerCreated(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On file or folder deleted",
        typeof(ObjectPermanentlyDeletedWebhookHandler),
        Description = "Triggered when an object is permanently deleted from the bucket.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectDeleted(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On file or folder restore completed",
        typeof(ObjectRestoreCompletedWebhookHandler),
        Description = "Triggered when a restore operation for a specific object is completed.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectRestoreCompleted(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On file or folder restore expired",
        typeof(ObjectRestoreExpiredWebhookHandler),
        Description = "Triggered when a restore operation for a specific object expires.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectRestoreExpired(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On file or folder restore initiated",
        typeof(ObjectRestoreInitiatedWebhookHandler),
        Description = "Triggered when a restore operation for a specific object is initiated.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectRestoreInitiated(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On file or folder tag added",
        typeof(ObjectTagAddedWebhookHandler),
        Description = "Triggered when a tag is added to a specific object.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectTagAdded(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On file or folder tag removed",
        typeof(ObjectTagRemovedWebhookHandler),
        Description = "Triggered when a tag is removed from a specific object.")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectTagRemoved(WebhookRequest request)
        => HandleWebhook(request);

    private Task<WebhookResponse<S3WebhookResponse>> HandleWebhook(WebhookRequest webhookRequest)
    {
        try
        {
            var payload = webhookRequest.Body.ToString();
            ArgumentException.ThrowIfNullOrEmpty(payload, nameof(webhookRequest.Body));

            var result = JsonConvert.DeserializeObject<S3WebhookPayload>(payload)!;

            return Task.FromResult(new WebhookResponse<S3WebhookResponse>
            {
                HttpResponseMessage = null,
                Result = new()
                {
                    Records = result.Records.Select(x => new RecordResponse()
                    {
                        BucketName = x.S3.Bucket.Name,
                        BucketArn = x.S3.Bucket.Arn,
                        ObjectKey = x.S3.Object.Key,
                        ObjectETag = x.S3.Object.ETag
                    })
                }
            });
        }
        catch (Exception ex)
        {
            var errorMessage = "[AmazonS3 webhook] Got an error while processing the webhook request. "
                + $"Request method: {webhookRequest.HttpMethod?.Method}"
                + $"Request body: {webhookRequest.Body}"
                + $"Exception message: {ex.Message}";

            InvocationContext.Logger?.LogError(errorMessage, [ex.Message]);
            throw;
        }
    }
}