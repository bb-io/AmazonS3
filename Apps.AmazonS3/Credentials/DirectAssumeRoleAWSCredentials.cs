using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace Apps.AmazonS3.Credentials;

internal sealed class DirectAssumeRoleAWSCredentials : RefreshingAWSCredentials
{
    private readonly AmazonSecurityTokenServiceClient _stsClient;
    private readonly string _roleArn;
    private readonly string _roleSessionName;
    private readonly string? _externalId;

    public DirectAssumeRoleAWSCredentials(
        AWSCredentials sourceCredentials,
        string roleArn,
        string roleSessionName,
        string? externalId,
        RegionEndpoint region)
    {
        _stsClient = new AmazonSecurityTokenServiceClient(sourceCredentials, region);
        _roleArn = roleArn;
        _roleSessionName = roleSessionName;
        _externalId = externalId;
        PreemptExpiryTime = TimeSpan.FromMinutes(15);
    }

    protected override CredentialsRefreshState GenerateNewCredentials()
    {
        return GenerateNewCredentialsAsync().GetAwaiter().GetResult();
    }

    protected override async Task<CredentialsRefreshState> GenerateNewCredentialsAsync()
    {
        var request = new AssumeRoleRequest
        {
            RoleArn = _roleArn,
            RoleSessionName = _roleSessionName
        };

        if (!string.IsNullOrWhiteSpace(_externalId))
        {
            request.ExternalId = _externalId;
        }

        var response = await _stsClient.AssumeRoleAsync(request).ConfigureAwait(false);
        var credentials = response.Credentials
            ?? throw new InvalidOperationException("AWS STS returned no credentials for the assumed role.");

        var immutableCredentials = new ImmutableCredentials(
            credentials.AccessKeyId,
            credentials.SecretAccessKey,
            credentials.SessionToken);
        var expiration = credentials.Expiration
            ?? throw new InvalidOperationException("AWS STS returned credentials without an expiration time.");

        return new CredentialsRefreshState(immutableCredentials, expiration);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stsClient.Dispose();
        }

        base.Dispose(disposing);
    }
}
