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
    [Webhook("On object created", typeof(ObjectCreatedWebhookHandler), Description = "On object created in the bucket")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectCreated(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On object delete marker created", typeof(ObjectDeleteMarkerCreatedWebhookHandler),
        Description = "On delete marker created for a specific objects")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectDeleteMarkerCreated(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On object deleted", typeof(ObjectPermanentlyDeletedWebhookHandler),
        Description = "On object permanently deleted")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectDeleted(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On object restore completed", typeof(ObjectRestoreCompletedWebhookHandler),
        Description = "On restore completed for a specific object")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectRestoreCompleted(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On object restore expired", typeof(ObjectRestoreExpiredWebhookHandler),
        Description = "On restore expired for a specific object")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectRestoreExpired(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On object restore initiated", typeof(ObjectRestoreInitiatedWebhookHandler),
        Description = "On restore initiated for a specific object")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectRestoreInitiated(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On object tag added", typeof(ObjectTagAddedWebhookHandler),
        Description = "On tag added for a specific object")]
    public Task<WebhookResponse<S3WebhookResponse>> OnObjectTagAdded(WebhookRequest request)
        => HandleWebhook(request);

    [Webhook("On object tag removed", typeof(ObjectTagRemovedWebhookHandler),
        Description = "On tag removed for a specific object")]
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