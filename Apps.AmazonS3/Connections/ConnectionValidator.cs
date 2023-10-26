using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.AmazonS3.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        var client = S3ClientFactory.CreateClient(authProviders.ToArray());

        try
        {
            await AmazonClientHandler.ExecuteS3Action(() => client.ListBucketsAsync(cancellationToken));

            return new()
            {
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}