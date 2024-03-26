using Amazon;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.AmazonS3.Factories;

public class AwsRegionHelper
{
    public static RegionEndpoint GetRegionEndpoint(string regionName)
    {
        // Validate the input to ensure it's not null or empty.
        if (string.IsNullOrWhiteSpace(regionName))
        {
            throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));
        }

        // Attempt to get the RegionEndpoint by system name.
        var regionEndpoint = RegionEndpoint.GetBySystemName(regionName);

        // Optional: Check if the regionEndpoint is null and handle the case where it might not be a valid AWS region system name.
        if (regionEndpoint == null)
        {
            throw new ArgumentException(
                $"The region name '{regionName}' is not a valid AWS region system name.",
                nameof(regionName)
            );
        }

        return regionEndpoint;
    }
}

public static class AmazonClientFactory
{
    public static AmazonS3Client CreateS3Client(
        AuthenticationCredentialsProvider[] creds,
        RegionEndpoint? defaultRegion = default
    )
    {
        var key = creds.Get("access_key").Value;
        var secret = creds.Get("access_secret").Value;
        var systemRegion = creds.Get("region").Value;

        if (systemRegion != null)
        {
            defaultRegion = AwsRegionHelper.GetRegionEndpoint(systemRegion);
        }

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new Exception(ExceptionMessages.CredentialsMissing);

        return new(
            key,
            secret,
            new AmazonS3Config { RegionEndpoint = defaultRegion ?? RegionEndpoint.USWest1 }
        );
    }

    // The client should have the same region as the bucket, so we have to find out
    // bucket's region in the first place and create new client with this region
    public static async Task<AmazonS3Client> CreateS3BucketClient(
        AuthenticationCredentialsProvider[] creds,
        string bucketName
    )
    {
        var client = CreateS3Client(creds);
        var locationResponse = await AmazonClientHandler.ExecuteS3Action(
            () => client.GetBucketLocationAsync(bucketName)
        );

        var regionEndpoint = RegionEndpoint.GetBySystemName(locationResponse.Location.Value);

        return CreateS3Client(creds, regionEndpoint);
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
