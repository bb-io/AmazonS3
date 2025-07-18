using Amazon.Runtime;
using Amazon.S3;
using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using Amazon;
using Amazon.SimpleNotificationService;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.AmazonS3;
public class AmazonInvocable : BaseInvocable
{
    public AmazonS3Client S3Client { get; }

    public AmazonInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        var key = invocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessKey);
        var secret = invocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessSecret);
        var region = invocationContext.AuthenticationCredentialsProviders.Get(CredNames.Region);

        if (string.IsNullOrEmpty(key.Value) || string.IsNullOrEmpty(secret.Value) || string.IsNullOrEmpty(region.Value))
            throw new PluginMisconfigurationException(ExceptionMessages.CredentialsMissing);

        S3Client = new(key.Value, secret.Value, new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region.Value)
        });
    }

    public async Task<AmazonS3Client> CreateBucketClient(string bucketName)
    {
        var key = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessKey);
        var secret = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.AccessSecret);

        string region;
        try
        {
            var locationResponse = await ExecuteAction(() => S3Client.GetBucketLocationAsync(bucketName));
            if (locationResponse.Location.Value is null || String.IsNullOrEmpty(locationResponse.Location.Value))
            { 
                region = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.Region).Value; 
            }
            else
            { 
                region = locationResponse.Location.Value; 
            }
        }
        catch (Exception)
        {
            region = InvocationContext.AuthenticationCredentialsProviders.Get(CredNames.Region).Value;
        }

        return new(key.Value, secret.Value, new AmazonS3Config
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
            throw new PluginMisconfigurationException(ExceptionMessages.CredentialsMissing);

        return new(key, secret, new AmazonSimpleNotificationServiceConfig()
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

    public async Task<IEnumerable<T>> ExecutePaginated<T, TResult>(IPaginatedEnumerable<TResult> paginatorResponses, Func<TResult, IEnumerable<T>> selector)
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
}

