using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Blackbird.Applications.Sdk.Utils.Webhooks.Bridge;
using Blackbird.Applications.Sdk.Utils.Webhooks.Bridge.Models.Request;
using EventType = Amazon.S3.EventType;

namespace Apps.AmazonS3.Webhooks.Handlers.Base;

public abstract class S3WebhookHandler : InvocableBridgeWebhookHandler
{
    protected abstract EventType Event { get; }
    private string Bucket { get; set; }

    public S3WebhookHandler(InvocationContext invocationContext, [WebhookParameter] BucketRequestModel bucketRequest) : base(
        invocationContext)
    {
        Bucket = bucketRequest.BucketName;
    }

    public override async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> creds,
        Dictionary<string, string> values)
    {
        var s3Client = await AmazonClientFactory.CreateS3BucketClient(creds.ToArray(), Bucket);

        var bucketNotifications = await s3Client.GetBucketNotificationAsync(Bucket);
        var topics = bucketNotifications.TopicConfigurations;

        if (!topics.Any(x => x.Events.Contains(Event)))
        {
            var snsClient = await AmazonClientFactory.CreateSNSClient(creds.ToArray(), s3Client.Config.RegionEndpoint);

            var allTopics = await snsClient.ListTopicsAsync();
            var blackbirdTopic =
                allTopics.Topics.FirstOrDefault(x => x.TopicArn.Contains(ApplicationConstants.SnsTopic));

            var topicArn = blackbirdTopic is not null
                ? blackbirdTopic.TopicArn
                : (await snsClient.CreateTopicAsync(
                    $"Blackbird-{ApplicationConstants.SnsTopic}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"))
                .TopicArn;

            await snsClient.AuthorizeS3ToPublishAsync(topicArn, Bucket);

            topics.Add(new()
            {
                Topic = topicArn,
                Events = new() { Event }
            });
            await s3Client.PutBucketNotificationAsync(new()
            {
                BucketName = Bucket,
                TopicConfigurations = topics
            });

            await snsClient.SubscribeAsync(new(topicArn, "https",
                $"{InvocationContext.UriInfo.BridgeServiceUrl}/webhooks/amazons3"));
        }

        await base.SubscribeAsync(creds, values);
    }

    public override async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> creds,
        Dictionary<string, string> values)
    {
        var s3Client = await AmazonClientFactory.CreateS3BucketClient(creds.ToArray(), Bucket);
        var bucketNotifications = await s3Client.GetBucketNotificationAsync(Bucket);

        var topics = bucketNotifications.TopicConfigurations;
        var eventToRemove = bucketNotifications.TopicConfigurations
            .FirstOrDefault(x => x.Events.Contains(Event) && x.Topic.Contains(ApplicationConstants.SnsTopic));

        if (eventToRemove is not null)
        {
            topics.Remove(eventToRemove);
            await s3Client.PutBucketNotificationAsync(new()
            {
                BucketName = Bucket,
                TopicConfigurations = topics
            });
        }

        await base.UnsubscribeAsync(creds, values);
    }

    protected override Task<(BridgeRequest webhookData, BridgeCredentials bridgeCreds)> GetBridgeServiceInputs(
        Dictionary<string, string> values, IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var webhookData = new BridgeRequest
        {
            Event = Event.ToString(),
            Id = Bucket,
            Url = values["payloadUrl"],
        };

        var bridgeCreds = new BridgeCredentials
        {
            ServiceUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl}/webhooks/amazons3",
            Token = ApplicationConstants.BlackbirdToken
        };

        return Task.FromResult((webhookData, bridgeCreds));
    }
}