using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<ConnectionValidationResponse>(new()
        {
            IsValid = true
        });
        }
    }
}