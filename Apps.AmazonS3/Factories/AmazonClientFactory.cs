using Amazon;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.AmazonS3.Factories;

public static class AmazonClientFactory
{
    public static AmazonS3Client CreateS3Client(
        AuthenticationCredentialsProvider[] creds,
        RegionEndpoint? region = default)
    {
        var key = creds.Get(CredNames.AccessKey).Value;
        var secret = creds.Get(CredNames.AccessSecret).Value;
        var systemRegion = creds.Get(CredNames.Region).Value;
        var defaultRegion = RegionEndpoint.GetBySystemName(systemRegion);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new Exception(ExceptionMessages.CredentialsMissing);

        return new(key, secret, new AmazonS3Config
        {
            RegionEndpoint = region ?? defaultRegion
        });
    }

    // The client should have the same region as the bucket, so we have to find out
    // bucket's region in the first place and create new client with this region
    public static async Task<AmazonS3Client> CreateS3BucketClient(
        AuthenticationCredentialsProvider[] creds,
        string bucketName)
    {
        var client = CreateS3Client(creds);

        string region;
        try
        {
            var locationResponse =
                await AmazonClientHandler.ExecuteS3Action(() => client.GetBucketLocationAsync(bucketName));
            if (locationResponse.Location.Value is null || String.IsNullOrEmpty(locationResponse.Location.Value))
            { region = creds.Get(CredNames.Region).Value; } else
            { region = locationResponse.Location.Value; }
        }
        catch (Exception)
        {
            region = creds.Get(CredNames.Region).Value;
        }

        return CreateS3Client(creds, RegionEndpoint.GetBySystemName(region));
    }

    public static async Task<AmazonSimpleNotificationServiceClient> CreateSNSClient(
        AuthenticationCredentialsProvider[] creds, RegionEndpoint? region = default)
    {
        var key = creds.Get(CredNames.AccessKey).Value;
        var secret = creds.Get(CredNames.AccessSecret).Value;
        var systemRegion = creds.Get(CredNames.Region).Value;
        var defaultRegion = RegionEndpoint.GetBySystemName(systemRegion);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new Exception(ExceptionMessages.CredentialsMissing);

        return new(key, secret, new AmazonSimpleNotificationServiceConfig()
        {
            RegionEndpoint = region ?? defaultRegion
        });
    }
}