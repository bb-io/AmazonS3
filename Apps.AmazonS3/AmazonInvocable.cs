using Amazon.Runtime;
using Amazon.S3;
using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using Amazon;
using Amazon.SimpleNotificationService;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Amazon.S3.Model;
using Amazon.SecurityToken.Model;

namespace Apps.AmazonS3;
public class AmazonInvocable : BaseInvocable
{
    public string CurrentConnectionType { get; }
    public AmazonS3Client S3Client { get; }
    public string ConnectedBucket { get; }

    public AmazonInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        CurrentConnectionType = invocationContext.AuthenticationCredentialsProviders.Get(CredNames.ConnectionType).Value
            ?? throw new PluginMisconfigurationException("Connection type is not set.");

        ConnectedBucket = CurrentConnectionType == ConnectionTypes.SingleBucket
            ? invocationContext.AuthenticationCredentialsProviders.Get(CredNames.Bucket).Value
                ?? throw new PluginMisconfigurationException("Connected bucket is not set for a single-bucket connection.")
            : string.Empty;

        var key = invocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessKey);
        var secret = invocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessSecret);
        var region = invocationContext.AuthenticationCredentialsProviders.Get(CredNames.Region);

        if (string.IsNullOrEmpty(key.Value) || string.IsNullOrEmpty(secret.Value) || string.IsNullOrEmpty(region.Value))
            throw new PluginMisconfigurationException("AWS User credentials missing. You need to specify access key and access secret to use Amazon S3");

        var credentials = BuildCredentials(invocationContext.AuthenticationCredentialsProviders);

        S3Client = new(credentials, new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region.Value)
        });
    }

    public async Task<AmazonS3Client> CreateBucketClient(string bucketName)
    {
        if (CurrentConnectionType == ConnectionTypes.SingleBucket)
            bucketName = ConnectedBucket;

        var key = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessKey);
        var secret = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessSecret);

        string region;
        try
        {
            var headBucketRequest = new HeadBucketRequest { BucketName = bucketName };
            var headBucketResponse = await ExecuteAction(() => S3Client.HeadBucketAsync(headBucketRequest));
            region = string.IsNullOrEmpty(headBucketResponse.BucketRegion)
                ? InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.Region).Value 
                : headBucketResponse.BucketRegion;
        }
        catch (Exception)
        {
            region = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.Region).Value;
        }

        var credentials = BuildCredentials(InvocationContext.AuthenticationCredentialsProviders);

        return new(credentials, new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region)
        });
    }

    public AmazonSimpleNotificationServiceClient CreateSnsClient(RegionEndpoint? region = default)
    {
        var key = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessKey).Value;
        var secret = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessSecret).Value;
        var systemRegion = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.Region).Value;
        var defaultRegion = RegionEndpoint.GetBySystemName(systemRegion);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new PluginMisconfigurationException("AWS User credentials missing. You need to specify access key and access secret to use Amazon S3");

        var credentials = BuildCredentials(InvocationContext.AuthenticationCredentialsProviders);

        return new(credentials, new AmazonSimpleNotificationServiceConfig()
        {
            RegionEndpoint = region ?? defaultRegion
        });
    }

    public static async Task<T> ExecuteAction<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException(ex.Message, ex);
        }
    }

    public static async Task ExecuteAction(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException(ex.Message, ex);
        }
    }

    public static async Task<IEnumerable<T>> ExecutePaginated<T, TResult>(IPaginatedEnumerable<TResult> paginatorResponses, Func<TResult, IEnumerable<T>> selector)
    {
        return await ExecuteAction(async () => {
            var response = new List<T>();
            await foreach (var page in paginatorResponses)
            {
                response.AddRange(selector(page));
            }
            return response;
        });
    }

    private AWSCredentials BuildCredentials(IEnumerable<Blackbird.Applications.Sdk.Common.Authentication.AuthenticationCredentialsProvider> authProviders)
    {
        var key = authProviders.Get(CredNames.AccessKey).Value;
        var secret = authProviders.Get(CredNames.AccessSecret).Value;

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new PluginMisconfigurationException("AWS User credentials missing. You need to specify access key and access secret to use Amazon S3");

        var basicCredentials = new BasicAWSCredentials(key, secret);

        var roleArn = authProviders.FirstOrDefault(x => x.KeyName == CredNames.RoleArn)?.Value;
        if (string.IsNullOrWhiteSpace(roleArn))
        {
            return basicCredentials;
        }

        var roleSessionName = "blackbird-session";
        var options = new AssumeRoleAWSCredentialsOptions();
        
        var externalId = authProviders.FirstOrDefault(x => x.KeyName == CredNames.ExternalId)?.Value;
        if (!string.IsNullOrWhiteSpace(externalId))
        {
            options.ExternalId = externalId;
        }
        
        return new AssumeRoleAWSCredentials(basicCredentials, roleArn, roleSessionName, options);
    }
}

