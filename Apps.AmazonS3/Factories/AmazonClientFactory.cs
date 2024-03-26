using Amazon;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.AmazonS3.Factories;

public static class AmazonClientFactory
{
    public static AmazonS3Client CreateS3Client(AuthenticationCredentialsProvider[] creds)
    {
        var key = creds.Get("access_key").Value;
        var secret = creds.Get("access_secret").Value;
        var systemRegion = creds.Get("region").Value;

        var defaultRegion = RegionEndpoint.GetBySystemName(systemRegion);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new Exception(ExceptionMessages.CredentialsMissing);

        return new(key, secret, new AmazonS3Config { RegionEndpoint = defaultRegion });
    }

    // The client should have the same region as the bucket, so we have to find out
    // bucket's region in the first place and create new client with this region
    public static async Task<AmazonS3Client> CreateS3BucketClient(
        AuthenticationCredentialsProvider[] creds,
        string bucketName
    )
    {
        return CreateS3Client(creds);
    }

    public static async Task<AmazonSimpleNotificationServiceClient> CreateSNSClient(
        AuthenticationCredentialsProvider[] creds,
        RegionEndpoint? region = default
    )
    {
        var key = creds.Get("access_key").Value;
        var secret = creds.Get("access_secret").Value;

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new Exception(ExceptionMessages.CredentialsMissing);

        return new(
            key,
            secret,
            new AmazonSimpleNotificationServiceConfig()
            {
                RegionEndpoint = region ?? RegionEndpoint.USWest1
            }
        );
    }
}
