using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            var context = new InvocationContext { AuthenticationCredentialsProviders = authProviders };
            var invocable = new AmazonInvocable(context);
            var result = await AmazonInvocable.ExecuteAction(() => invocable.S3Client.ListBucketsAsync(cancellationToken));

            return new() { IsValid = true };
        }
        catch (Exception ex)
        {
            return new() { IsValid = false, Message = ex.Message };
        }
    }
}