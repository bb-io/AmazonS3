using Amazon;
using Amazon.S3;
using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.AmazonS3.Factories;

public static class S3ClientFactory
{
    public static AmazonS3Client CreateClient(
        AuthenticationCredentialsProvider[] authenticationCredentialsProviders,
        RegionEndpoint? region = default)
    {
        var key = authenticationCredentialsProviders.First(p => p.KeyName == "access_key");
        var secret = authenticationCredentialsProviders.First(p => p.KeyName == "access_secret");

        if (string.IsNullOrEmpty(key.Value) || string.IsNullOrEmpty(secret.Value))
            throw new Exception(ExceptionMessages.CredentialsMissing);

        return new(key.Value, secret.Value, new AmazonS3Config
        {
            RegionEndpoint = region ?? RegionEndpoint.USWest1
        });
    }

    // The client should have the same region as the bucket, so we have to find out
    // bucket's region in the first place and create new client with this region
    public static async Task<AmazonS3Client> CreateBucketClient(
        AuthenticationCredentialsProvider[] authenticationCredentialsProviders,
        string bucketName)
    {
        var client = CreateClient(authenticationCredentialsProviders);
        var locationResponse = await client.GetBucketLocationAsync(bucketName);

        var regionEndpoint = RegionEndpoint.GetBySystemName(locationResponse.Location.Value);

        return CreateClient(authenticationCredentialsProviders, regionEndpoint);
    }
}